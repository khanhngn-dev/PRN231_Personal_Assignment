using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositorty.UnitOfWork;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using WebAPI.Models.Response;
using WebAPI.Models.Request;
using WebAPI.Constants;
using Repositorty.Entities;

namespace WebAPI.Controllers
{
    [Route("api/students")]
    // Staff role is 2
    [Authorize(Roles = Role.STAFF)]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudentsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/students
        [HttpGet]
        public ActionResult<PaginationResponse<Student>> GetStudents([FromQuery] int? groupId, [FromQuery] int? minBirthYear, [FromQuery] int? maxBirthYear, [FromQuery] int? pageSize, [FromQuery] int? pageIndex)
        {
            var students = _unitOfWork.GetRepository<Student>()
                    .FindByCondition(s =>
                        (groupId == null || s.GroupId == groupId)
                        && (minBirthYear == null || (s.DateOfBirth != null && s.DateOfBirth.Value.Year >= minBirthYear))
                        && (maxBirthYear == null || (s.DateOfBirth != null && s.DateOfBirth.Value.Year <= maxBirthYear)),
                        pageIndex,
                        pageSize,
                        s => s.Group)
                    .Select(s => new Student
                    {
                        Id = s.Id,
                        DateOfBirth = s.DateOfBirth,
                        Email = s.Email,
                        FullName = s.FullName,
                        GroupId = s.GroupId,
                        Group = new StudentGroup
                        {
                            Code = s.Group.Code,
                            GroupName = s.Group.GroupName,
                            Id = s.Group.Id
                        }
                    });

            var total = _unitOfWork.GetRepository<Student>()
                    .FindByCondition(s =>
                        (groupId == null || s.GroupId == groupId)
                        && (minBirthYear == null || (s.DateOfBirth != null && s.DateOfBirth.Value.Year >= minBirthYear))
                        && (maxBirthYear == null || (s.DateOfBirth != null && s.DateOfBirth.Value.Year <= maxBirthYear)))
                    .Count();

            return new PaginationResponse<Student>
            {
                Items = students.ToList(),
                PageIndex = pageIndex ?? 1,
                PageSize = pageSize ?? 10,
                TotalItems = total
            };
        }

        // GET: api/students/{id}
        [HttpGet("{id}")]
        public ActionResult<DetailStudentResponse> GetStudent(int id)
        {
            var student = _unitOfWork.GetRepository<Student>().FindByCondition(s => s.Id == id, s => s.Group).FirstOrDefault();

            if (student is null)
            {
                return NotFound();
            }

            return new DetailStudentResponse
            {
                Id = student.Id,
                Email = student.Email,
                DateOfBirth = student.DateOfBirth,
                FullName = student.FullName,
                GroupId = student.GroupId,
                GroupName = student.Group.GroupName
            };
        }

        // PUT: api/students/{id}
        [HttpPut("{id}")]
        public ActionResult<Student> PutStudent([FromRoute] int id, [FromBody] UpdateStudentRequest body)
        {
            var student = _unitOfWork.GetRepository<Student>().FindByCondition(s => s.Id == id, s => s.Group).FirstOrDefault();

            if (student is null)
            {
                return NotFound();
            }

            // Email is unique
            if (body.Email is not null && _unitOfWork.GetRepository<Student>().FindByCondition(s => s.Email == body.Email && s.Id != id).FirstOrDefault() is not null)
            {
                return BadRequest("Email is already taken");
            }

            // GroupId exists
            if (body.GroupId is not null && _unitOfWork.GetRepository<StudentGroup>().FindByCondition(g => g.Id == body.GroupId).FirstOrDefault() is null)
            {
                return BadRequest("Group does not exist");
            }

            student.Email = body.Email ?? student.Email;
            student.DateOfBirth = body.DateOfBirth ?? student.DateOfBirth;
            student.FullName = body.FullName ?? student.FullName;
            student.GroupId = body.GroupId ?? student.GroupId;

            var isValid = ValidateStudent(student);
            if (!string.IsNullOrEmpty(isValid))
            {
                return BadRequest(isValid);
            }

            try
            {
                _unitOfWork.GetRepository<Student>().Update(student);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }

            return new Student
            {
                Id = student.Id,
                Email = student.Email,
                FullName = student.FullName,
                DateOfBirth = student.DateOfBirth,
                GroupId = student.GroupId,
                Group = new StudentGroup
                {
                    Code = student.Group.Code,
                    GroupName = student.Group.GroupName,
                    Id = student.Group.Id
                }
            };
        }

        // POST: api/students
        [HttpPost]
        public ActionResult<Student> PostStudent([FromBody] CreateStudentRequest body)
        {
            // Email is unique
            if (_unitOfWork.GetRepository<Student>().FindByCondition(s => s.Email == body.Email).FirstOrDefault() is not null)
            {
                return BadRequest("Email is already taken");
            }

            // GroupId exists
            if (_unitOfWork.GetRepository<StudentGroup>().FindByCondition(g => g.Id == body.GroupId).FirstOrDefault() is null)
            {
                return BadRequest("Group does not exist");
            }

            var student = new Student
            {
                Email = body.Email,
                FullName = body.FullName,
                DateOfBirth = body.DateOfBirth,
                GroupId = body.GroupId
            };

            var isValid = ValidateStudent(student);
            if (!string.IsNullOrEmpty(isValid))
            {
                return BadRequest(isValid);
            }

            try
            {
                _unitOfWork.GetRepository<Student>().Insert(student);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }

            return new Student
            {
                Id = student.Id,
                Email = student.Email,
                FullName = student.FullName,
                DateOfBirth = body.DateOfBirth,
                GroupId = student.GroupId,
                Group = new StudentGroup
                {
                    Code = student.Group.Code,
                    GroupName = student.Group.GroupName,
                    Id = student.Group.Id
                }
            };
        }

        // DELETE: api/students/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            var student = _unitOfWork.GetRepository<Student>().FindByCondition(s => s.Id == id).FirstOrDefault();
            if (student is null)
            {
                return NotFound();
            }

            try
            {
                _unitOfWork.GetRepository<Student>().Delete(student);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }

            return Ok();
        }

        private string ValidateStudent(Student student)
        {
            if (student.DateOfBirth is null || student.FullName is null || student.Email is null || student.GroupId is null)
            {
                return "Missing information";
            }
            // Age between 18 and 100
            if (student.DateOfBirth.Value.AddYears(18) > DateTime.Now || student.DateOfBirth.Value.AddYears(100) < DateTime.Now)
            {
                return "Must be between 18 and 100 years old";
            }
            // Each word in fullname starts with an uppercase letter
            if (!Regex.IsMatch(student.FullName.Trim(), @"^([A-Z][a-z]*\s*)+$"))
            {
                return "Name must start with an uppercase letter and only contain letters and spaces";
            }
            return string.Empty;
        }

        // GET: api/students/groups
        [HttpGet("groups")]
        public ActionResult<IEnumerable<StudentGroup>> GetStudentGroups()
        {
            return _unitOfWork.GetRepository<StudentGroup>().Get().Select(g => new StudentGroup
            {
                Code = g.Code,
                GroupName = g.GroupName,
                Id = g.Id
            }).ToList();
        }
    }
}
