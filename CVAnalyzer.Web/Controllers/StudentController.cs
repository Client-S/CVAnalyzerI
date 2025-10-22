using CVAnalyzer.Application.DTOs.Student;
using CVAnalyzer.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVAnalyzer.Web.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly ICVProcessingService _cvProcessingService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(
            IStudentService studentService,
            ICVProcessingService cvProcessingService,
            ILogger<StudentController> logger)
        {
            _studentService = studentService;
            _cvProcessingService = cvProcessingService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var students = await _studentService.GetAllStudentsAsync();
                return View(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading students");
                TempData["ErrorMessage"] = "Error loading students.";
                return View(new List<StudentDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);

                if (student == null)
                {
                    TempData["ErrorMessage"] = "Student not found.";
                    return RedirectToAction("Index");
                }

                var cvDocuments = await _cvProcessingService.GetStudentCVsAsync(id);
                ViewBag.CVDocuments = cvDocuments;

                return View(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading student details {StudentId}", id);
                TempData["ErrorMessage"] = "Error loading student details.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateStudentDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CreateStudentDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var student = await _studentService.CreateStudentAsync(model);
                TempData["SuccessMessage"] = "Student created successfully.";
                return RedirectToAction("Details", new { id = student.Id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student");
                TempData["ErrorMessage"] = "An error occurred while creating the student.";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);

                if (student == null)
                {
                    TempData["ErrorMessage"] = "Student not found.";
                    return RedirectToAction("Index");
                }

                var updateDto = new UpdateStudentDto
                {
                    Id = student.Id,
                    Name = student.Name,
                    Email = student.Email,
                    Phone = student.Phone
                };

                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading student for edit {StudentId}", id);
                TempData["ErrorMessage"] = "Error loading student.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateStudentDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _studentService.UpdateStudentAsync(model);

                if (result)
                {
                    TempData["SuccessMessage"] = "Student updated successfully.";
                    return RedirectToAction("Details", new { id = model.Id });
                }

                TempData["ErrorMessage"] = "Student not found.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student {StudentId}", model.Id);
                TempData["ErrorMessage"] = "An error occurred while updating the student.";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _studentService.DeleteStudentAsync(id);

                if (result)
                    TempData["SuccessMessage"] = "Student deleted successfully.";
                else
                    TempData["ErrorMessage"] = "Student not found.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student {StudentId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the student.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return RedirectToAction("Index");

            try
            {
                var students = await _studentService.SearchStudentsAsync(searchTerm);
                ViewBag.SearchTerm = searchTerm;
                return View("Index", students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching students with term: {SearchTerm}", searchTerm);
                TempData["ErrorMessage"] = "Error searching students.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> BySkill(string skill)
        {
            if (string.IsNullOrWhiteSpace(skill))
                return RedirectToAction("Index");

            try
            {
                var students = await _studentService.GetStudentsBySkillAsync(skill);
                ViewBag.Skill = skill;
                ViewBag.Title = $"Students with {skill} skill";
                return View("Index", students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students by skill: {Skill}", skill);
                TempData["ErrorMessage"] = "Error loading students.";
                return RedirectToAction("Index");
            }
        }

        // API endpoints for AJAX calls
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var students = await _studentService.GetAllStudentsAsync();
                return Json(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all students");
                return Json(new { error = "Failed to load students" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                return Json(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student {StudentId}", id);
                return Json(new { error = "Failed to load student" });
            }
        }
    }
}
