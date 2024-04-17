using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessObjects;
using Repositorty.UnitOfWork;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebAPI.Controllers
{
    [Route("api/students")]
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
        public ActionResult<IEnumerable<Student>> GetStudents()
        {
            var students = _unitOfWork.GetRepository<Student>().Get(null, null, s => s.Group).Select(s => new Student
            {
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
            return students.ToList();
        }

        // GET: api/students/5
        [HttpGet("{id}")]
        public ActionResult<Student> GetStudent(int id)
        {
            var student = _unitOfWork.GetRepository<Student>().FindByCondition(s => s.Id == id, s => s.Group).FirstOrDefault();

            if (student == null)
            {
                return NotFound();
            }

            return new Student
            {
                Id = student.Id,
                Email = student.Email,
                DateOfBirth = student.DateOfBirth,
                FullName = student.FullName,
                GroupId = student.GroupId,
                Group = new StudentGroup
                {
                    Code = student.Group.Code,
                    GroupName = student.Group.GroupName,
                    Id = student.Group.Id
                }
            };
        }

        public class StudentUpdateDTO
        {
            [EmailAddress]
            public string? Email { get; set; }
            [MinLength(1)]
            public string? FullName { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public int? GroupId { get; set; }
        }

        // PUT: api/students/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public IActionResult PutStudent([FromRoute] int id, [FromBody] StudentUpdateDTO body)
        {
            var student = _unitOfWork.GetRepository<Student>().FindByCondition(s => s.Id == id, s => s.Group).FirstOrDefault();

            if (student == null)
            {
                return NotFound();
            }

            // Email is unique
            if (body.Email != null && _unitOfWork.GetRepository<Student>().FindByCondition(s => s.Email == body.Email && s.Id != id).FirstOrDefault() != null)
            {
                return BadRequest("Email is already taken");
            }

            // GroupId exists
            if (body.GroupId != null && _unitOfWork.GetRepository<StudentGroup>().FindByCondition(g => g.Id == body.GroupId).FirstOrDefault() == null)
            {
                return BadRequest("Group does not exist");
            }

            student.Email = body.Email ?? student.Email;
            student.DateOfBirth = body.DateOfBirth ?? student.DateOfBirth;
            student.FullName = body.FullName ?? student.FullName;
            student.GroupId = body.GroupId ?? student.GroupId;

            if (!ValidateStudent(student))
            {
                return BadRequest("Invalid student information");
            }

            try
            {
                _unitOfWork.GetRepository<Student>().Update(student);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }

            return CreatedAtAction("GetStudent", new { id = student.Id }, new Student
            {
                Id = student.Id,
                Email = student.Email,
                DateOfBirth = student.DateOfBirth,
                FullName = student.FullName,
                GroupId = student.GroupId,
                Group = new StudentGroup
                {
                    Code = student.Group.Code,
                    GroupName = student.Group.GroupName,
                    Id = student.Group.Id
                }
            });
        }
        public class StudentCreateDTO
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            [Required]
            [MinLength(1)]
            public string FullName { get; set; }
            [Required]
            public DateTime DateOfBirth { get; set; }
            [Required]
            public int GroupId { get; set; }
        }

        // POST: api/students
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public ActionResult<Student> PostStudent([FromBody] StudentCreateDTO body)
        {
            // Email is unique
            if (_unitOfWork.GetRepository<Student>().FindByCondition(s => s.Email == body.Email).FirstOrDefault() != null)
            {
                return BadRequest("Email is already taken");
            }

            // GroupId exists
            if (_unitOfWork.GetRepository<StudentGroup>().FindByCondition(g => g.Id == body.GroupId).FirstOrDefault() == null)
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

            if (!ValidateStudent(student))
            {
                return BadRequest("Invalid student information");
            }

            _unitOfWork.GetRepository<Student>().Insert(student);
            _unitOfWork.Save();

            return CreatedAtAction("GetStudent", new { id = student.Id }, new Student
            {
                Id = student.Id,
                Email = student.Email,
                DateOfBirth = student.DateOfBirth,
                FullName = student.FullName,
                GroupId = student.GroupId,
                Group = new StudentGroup
                {
                    Code = student.Group.Code,
                    GroupName = student.Group.GroupName,
                    Id = student.Group.Id
                }
            });
        }

        // DELETE: api/students/5
        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            var student = _unitOfWork.GetRepository<Student>().FindByCondition(s => s.Id == id).FirstOrDefault();
            if (student == null)
            {
                return NotFound();
            }

            _unitOfWork.GetRepository<Student>().Delete(student);
            _unitOfWork.Save();

            return NoContent();
        }

        private bool ValidateStudent(Student student)
        {
            if (student.DateOfBirth == null || student.FullName == null || student.Email == null || student.GroupId == null)
            {
                return false;
            }
            // Age between 18 and 100
            if (student.DateOfBirth.Value.AddYears(18) > DateTime.Now || student.DateOfBirth.Value.AddYears(100) < DateTime.Now)
            {
                return false;
            }
            // Each word in fullname starts with an uppercase letter
            if (!Regex.IsMatch(student.FullName.Trim(), @"^([A-Z][a-z]*\s*)+$"))
            {
                return false;
            }
            return true;
        }
    }
}
