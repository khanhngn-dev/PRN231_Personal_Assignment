using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BusinessObjects;
using System.Net.Http.Headers;
using System.Text.Json;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class StudentsController : Controller
    {
        private readonly HttpClient client;
        public StudentsController()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private void AttachToken()
        {
            var token = HttpContext.Session.GetString("Token");
            if (token != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        private T Deserialize<T>(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<T>(json, options);
        }

        // GET: Students
        public async Task<ActionResult<PaginationModel<Student>>> Index([FromQuery] int? groupId, int? minBirthYear, int? maxBirthYear, int? pageSize, int? pageIndex)
        {
            AttachToken();
            var response = await client.GetAsync($"https://localhost:7009/api/students?groupId={groupId}&minBirthYear={minBirthYear}&maxBirthYear={maxBirthYear}&pageSize={pageSize}&pageIndex={pageIndex}");
            try
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Index", "Login");
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return RedirectToAction("Error", "Login", new { message = "Only staff are allowed to view this page", code = 403 });
                }
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                return RedirectToAction("Error", "Login", new { message = ex.Message, status = ex.StatusCode });
            }
            var stringData = await response.Content.ReadAsStringAsync();
            var students = Deserialize<PaginationModel<Student>>(stringData);
            return View(students);
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            AttachToken();
            var response = await client.GetAsync($"https://localhost:7009/api/students/{id}");
            try
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Index", "Login");
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return RedirectToAction("Error", "Login", new { message = "Only staff are allowed to view this page", code = 403 });
                }
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                return RedirectToAction("Error", "Login", new { message = ex.Message, status = ex.StatusCode });
            }

            var stringData = await response.Content.ReadAsStringAsync();
            var student = Deserialize<StudentDetailModel>(stringData);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        private async Task<IEnumerable<StudentGroup>> GetStudentGroups()
        {
            var response = await client.GetAsync("https://localhost:7009/api/students/groups");
            if (!response.IsSuccessStatusCode)
            {
                return Enumerable.Empty<StudentGroup>();
            }
            var stringData = await response.Content.ReadAsStringAsync();
            return Deserialize<IEnumerable<StudentGroup>>(stringData);
        }

        // GET: Students/Create
        public async Task<IActionResult> Create()
        {
            AttachToken();
            var studentGroups = await GetStudentGroups();
            if (!studentGroups.Any())
            {
                return RedirectToAction("Error", "Login", new { message = "No student groups found", code = 404 });
            }
            ViewData["GroupId"] = new SelectList(await GetStudentGroups(), "Id", "Code");
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,FullName,DateOfBirth,GroupId")] Student student)
        {
            AttachToken();
            if (ModelState.IsValid)
            {
                var response = await client.PostAsJsonAsync("https://localhost:7009/api/students", student);
                try
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Index", "Login");
                    }
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return RedirectToAction("Error", "Login", new { message = "Only staff are allowed to view this page", code = 403 });
                    }
                    response.EnsureSuccessStatusCode();
                    return RedirectToAction(nameof(Index));
                }
                catch (HttpRequestException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            ViewData["GroupId"] = new SelectList(await GetStudentGroups(), "Id", "Code", student.GroupId);
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            AttachToken();
            var response = await client.GetAsync($"https://localhost:7009/api/students/{id}");
            try
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Index", "Login");
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return RedirectToAction("Error", "Login", new { message = "Only staff are allowed to view this page", code = 403 });
                }
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                return RedirectToAction("Error", "Login", new { message = ex.Message, status = ex.StatusCode });
            }
            var stringData = await response.Content.ReadAsStringAsync();
            var student = Deserialize<Student>(stringData);
            if (student == null)
            {
                return NotFound();
            }
            ViewData["GroupId"] = new SelectList(await GetStudentGroups(), "Id", "Code", student.GroupId);
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,FullName,DateOfBirth,GroupId")] Student student)
        {
            AttachToken();
            if (ModelState.IsValid)
            {
                var response = await client.PutAsJsonAsync($"https://localhost:7009/api/students/{id}", student);
                try
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return RedirectToAction("Index", "Login");
                    }
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return RedirectToAction("Error", "Login", new { message = "Only staff are allowed to view this page", code = 403 });
                    }
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return NotFound();
                    }
                    response.EnsureSuccessStatusCode();
                    return RedirectToAction(nameof(Index));
                }
                catch (HttpRequestException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            ViewData["GroupId"] = new SelectList(await GetStudentGroups(), "Id", "Code", student.GroupId);
            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            AttachToken();
            var response = await client.GetAsync($"https://localhost:7009/api/students/{id}");
            try
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Index", "Login");
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return RedirectToAction("Error", "Login", new { message = "Only staff are allowed to view this page", code = 403 });
                }
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                return RedirectToAction("Error", "Login", new { message = ex.Message, status = ex.StatusCode });
            }
            var stringData = await response.Content.ReadAsStringAsync();
            var student = Deserialize<Student>(stringData);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            AttachToken();
            var response = await client.DeleteAsync($"https://localhost:7009/api/students/{id}");
            try
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Index", "Login");
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return RedirectToAction("Error", "Login", new { message = "Only staff are allowed to view this page", code = 403 });
                }
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                response.EnsureSuccessStatusCode();
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                return RedirectToAction("Error", "Login", new { message = ex.Message, status = ex.StatusCode });
            }
        }
    }
}
