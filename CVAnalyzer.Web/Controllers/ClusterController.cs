using CVAnalyzer.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVAnalyzer.Web.Controllers
{
    [Authorize(Roles = "Admin,Faculty")]
    public class ClusterController : Controller
    {
        private readonly IClusteringService _clusteringService;
        private readonly IMLClusteringService _mlClusteringService; 
        private readonly ILogger<ClusterController> _logger;

        public ClusterController(
            IClusteringService clusteringService,
            IMLClusteringService mlClusteringService, 
            ILogger<ClusterController> logger)
        {
            _clusteringService = clusteringService;
            _mlClusteringService = mlClusteringService; 
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var clusters = await _clusteringService.GetAllClustersAsync();
                return View(clusters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading clusters");
                TempData["ErrorMessage"] = "Error loading clusters.";
                return View(new List<CVAnalyzer.Application.DTOs.Cluster.ClusterDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var cluster = await _clusteringService.GetClusterDetailsAsync(id);

                if (cluster == null)
                {
                    TempData["ErrorMessage"] = "Cluster not found.";
                    return RedirectToAction("Index");
                }

                return View(cluster);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cluster details {ClusterId}", id);
                TempData["ErrorMessage"] = "Error loading cluster details.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CreateML()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string clusterName, int numberOfGroups = 3)
        {
            if (string.IsNullOrWhiteSpace(clusterName))
            {
                TempData["ErrorMessage"] = "Please enter a cluster name.";
                return View();
            }

            if (numberOfGroups < 2 || numberOfGroups > 10)
            {
                TempData["ErrorMessage"] = "Number of groups must be between 2 and 10.";
                return View();
            }

            try
            {
                var cluster = await _clusteringService.CreateSimpleClusterAsync(clusterName, numberOfGroups);
                TempData["SuccessMessage"] = $"Cluster '{clusterName}' created successfully with {cluster.Students.Count} students!";
                return RedirectToAction("Details", new { id = cluster.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cluster");
                TempData["ErrorMessage"] = ex.Message;
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateKMeans(string clusterName, int numberOfClusters = 3)
        {
            if (string.IsNullOrWhiteSpace(clusterName))
            {
                TempData["ErrorMessage"] = "Please enter a cluster name.";
                return View("CreateML");
            }

            try
            {
                var cluster = await _mlClusteringService.CreateKMeansClusterAsync(clusterName, numberOfClusters);
                TempData["SuccessMessage"] = $"K-Means cluster '{clusterName}' created with ML.NET! {cluster.Students.Count} students grouped.";
                return RedirectToAction("Details", new { id = cluster.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating K-Means cluster");
                TempData["ErrorMessage"] = ex.Message;
                return View("CreateML");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDBSCAN(string clusterName, double epsilon = 0.5, int minPoints = 2)
        {
            if (string.IsNullOrWhiteSpace(clusterName))
            {
                TempData["ErrorMessage"] = "Please enter a cluster name.";
                return View("CreateML");
            }

            try
            {
                var cluster = await _mlClusteringService.CreateDBSCANClusterAsync(clusterName, epsilon, minPoints);
                TempData["SuccessMessage"] = $"DBSCAN cluster '{clusterName}' created with ML.NET! Found natural groupings.";
                return RedirectToAction("Details", new { id = cluster.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating DBSCAN cluster");
                TempData["ErrorMessage"] = ex.Message;
                return View("CreateML");
            }
        }

        [HttpGet]
        public async Task<IActionResult> SimilarSkills(string skill)
        {
            if (string.IsNullOrWhiteSpace(skill))
                return Json(new List<string>());

            try
            {
                var similar = await _mlClusteringService.FindSimilarSkillsAsync(skill);
                return Json(similar);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding similar skills for {Skill}", skill);
                return Json(new List<string>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _clusteringService.DeleteClusterAsync(id);

                if (result)
                    TempData["SuccessMessage"] = "Cluster deleted successfully.";
                else
                    TempData["ErrorMessage"] = "Cluster not found.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cluster {ClusterId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the cluster.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> FindSimilar(int studentId)
        {
            try
            {
                var similarStudents = await _clusteringService.FindSimilarStudentsAsync(studentId);
                ViewBag.StudentId = studentId;
                return View(similarStudents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding similar students for {StudentId}", studentId);
                TempData["ErrorMessage"] = "Error finding similar students.";
                return RedirectToAction("Index", "Student");
            }
        }
    }

}
