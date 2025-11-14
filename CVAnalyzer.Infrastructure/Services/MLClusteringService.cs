using CVAnalyzer.Application.DTOs.Cluster;
using CVAnalyzer.Application.Models;
using CVAnalyzer.Application.Services;
using CVAnalyzer.Core.Entities;
using CVAnalyzer.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Infrastructure.Services
{
    public class MLClusteringService : IMLClusteringService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MLClusteringService> _logger;
        private readonly MLContext _mlContext;

        // Skill similarity mapping for semantic matching
        private readonly Dictionary<string, List<string>> _skillSynonyms = new()
        {
            { "JavaScript", new() { "JS", "ECMAScript", "Javascript", "javascript" } },
            { "Python", new() { "Python3", "Py", "python" } },
            { "C#", new() { "CSharp", "C Sharp", "csharp" } },
            { "Java", new() { "java" } },
            { "React", new() { "ReactJS", "React.js", "react" } },
            { "Angular", new() { "AngularJS", "Angular.js", "angular" } },
            { "Vue", new() { "VueJS", "Vue.js", "vue" } },
            { "Node.js", new() { "NodeJS", "Node", "node" } },
            { "ASP.NET", new() { "ASPNET", "ASP.NET Core", "aspnet" } },
            { "SQL Server", new() { "MSSQL", "MS SQL", "Microsoft SQL Server", "sqlserver" } },
            { "MongoDB", new() { "Mongo", "mongo", "mongodb" } },
            { "PostgreSQL", new() { "Postgres", "postgres", "postgresql" } },
            { "Docker", new() { "docker" } },
            { "Kubernetes", new() { "K8s", "k8s", "kubernetes" } },
            { "AWS", new() { "Amazon Web Services", "aws" } },
            { "Azure", new() { "Microsoft Azure", "azure" } },
            { "Git", new() { "git", "GitHub", "GitLab" } },
            { "Machine Learning", new() { "ML", "ml" } },
            { "Artificial Intelligence", new() { "AI", "ai" } }
        };

        public MLClusteringService(
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<MLClusteringService> logger)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _mlContext = new MLContext(seed: 0);
        }

        public async Task<ClusterDetailDto> CreateKMeansClusterAsync(string clusterName, int numberOfClusters)
        {
            try
            {
                _logger.LogInformation("Starting K-Means clustering with {K} clusters", numberOfClusters);

                // Get all students with skills
                var students = (await _unitOfWork.Students.GetStudentsWithSkillsAsync()).ToList();

                if (students.Count == 0)
                    throw new InvalidOperationException("No students found to cluster");

                if (numberOfClusters > students.Count)
                    throw new InvalidOperationException($"Number of clusters ({numberOfClusters}) cannot exceed number of students ({students.Count})");

                // Normalize skills using semantic matching
                var normalizedStudents = NormalizeStudentSkills(students);

                // Create feature vectors for ML
                var studentFeatures = CreateFeatureVectors(normalizedStudents);

                if (studentFeatures.Count == 0)
                    throw new InvalidOperationException("No feature vectors could be created for clustering");

                // Prepare data for ML.NET
                var trainingData = _mlContext.Data.LoadFromEnumerable(
                    studentFeatures.Select(sf => new StudentClusterData
                    {
                        Features = sf.SkillVector
                    }));

                // Build K-Means clustering pipeline
                var pipeline = _mlContext.Transforms
                    .Concatenate("Features", nameof(StudentClusterData.Features))
                    .Append(_mlContext.Clustering.Trainers.KMeans(
                        featureColumnName: "Features",
                        numberOfClusters: numberOfClusters));

                // Train the model
                _logger.LogInformation("Training K-Means model...");
                var model = pipeline.Fit(trainingData);

                // Get predictions
                var predictions = model.Transform(trainingData);
                var clusterAssignments = _mlContext.Data
                    .CreateEnumerable<ClusterPrediction>(predictions, reuseRowObject: false)
                    .ToList();

                // Create cluster in database
                var userId = GetCurrentUserId();
                var cluster = new StudentCluster
                {
                    ClusterName = clusterName,
                    Algorithm = "K-Means (ML.NET)",
                    Description = $"K-Means clustering with {numberOfClusters} clusters using ML.NET",
                    MemberCount = students.Count,
                    CreatedByUserId = userId,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Clusters.AddAsync(cluster);
                await _unitOfWork.SaveChangesAsync();

                // Assign students to clusters
                for (int i = 0; i < students.Count; i++)
                {
                    var student = students[i];
                    var prediction = clusterAssignments[i];
                    var studentSkills = normalizedStudents.First(s => s.Id == student.Id).StudentSkills;

                    var member = new ClusterMember
                    {
                        ClusterId = cluster.Id,
                        StudentId = student.Id,
                        SimilarityScore = CalculateSilhouetteScore(
                            studentFeatures[i].SkillVector,
                            prediction.PredictedClusterId,
                            studentFeatures,
                            clusterAssignments),
                        MatchingSkills = string.Join(",",
                            studentSkills.Select(ss => ss.Skill.SkillName))
                    };

                    cluster.Members.Add(member);
                }

                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogAsync("CreateMLCluster", "Cluster", cluster.Id.ToString(),
                    new { Algorithm = "K-Means", Clusters = numberOfClusters, Students = students.Count });

                _logger.LogInformation("K-Means clustering completed successfully");

                return await GetClusterDetails(cluster.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating K-Means cluster");
                throw;
            }
        }

        public async Task<ClusterDetailDto> CreateDBSCANClusterAsync(string clusterName, double epsilon = 0.5, int minPoints = 2)
        {
            try
            {
                _logger.LogInformation("Starting DBSCAN clustering with epsilon={Epsilon}, minPoints={MinPoints}",
                    epsilon, minPoints);

                var students = (await _unitOfWork.Students.GetStudentsWithSkillsAsync()).ToList();

                if (students.Count == 0)
                    throw new InvalidOperationException("No students found to cluster");

                // Normalize skills
                var normalizedStudents = NormalizeStudentSkills(students);
                var studentFeatures = CreateFeatureVectors(normalizedStudents);

                if (studentFeatures.Count == 0)
                    throw new InvalidOperationException("No feature vectors could be created for clustering");

                // Implement DBSCAN algorithm
                var clusterAssignments = PerformDBSCAN(studentFeatures, epsilon, minPoints);

                // Create cluster in database
                var userId = GetCurrentUserId();
                var distinctClusters = clusterAssignments.Select(c => c.ClusterId).Distinct().Count();

                var cluster = new StudentCluster
                {
                    ClusterName = clusterName,
                    Algorithm = "DBSCAN (ML.NET-based)",
                    Description = $"DBSCAN clustering found {distinctClusters} natural clusters (ε={epsilon}, minPts={minPoints})",
                    MemberCount = students.Count,
                    CreatedByUserId = userId,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Clusters.AddAsync(cluster);
                await _unitOfWork.SaveChangesAsync();

                // Assign students to clusters
                for (int i = 0; i < students.Count; i++)
                {
                    var student = students[i];
                    var assignment = clusterAssignments[i];
                    var studentSkills = normalizedStudents.First(s => s.Id == student.Id).StudentSkills;

                    var member = new ClusterMember
                    {
                        ClusterId = cluster.Id,
                        StudentId = student.Id,
                        SimilarityScore = assignment.IsCore ? 100 : (assignment.ClusterId == -1 ? 0 : 50),
                        MatchingSkills = string.Join(",",
                            studentSkills.Select(ss => ss.Skill.SkillName))
                    };

                    cluster.Members.Add(member);
                }

                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogAsync("CreateMLCluster", "Cluster", cluster.Id.ToString(),
                    new { Algorithm = "DBSCAN", Clusters = distinctClusters, Students = students.Count });

                _logger.LogInformation("DBSCAN clustering completed successfully");

                return await GetClusterDetails(cluster.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating DBSCAN cluster");
                throw;
            }
        }

        public async Task<List<string>> FindSimilarSkillsAsync(string skill, int topN = 5)
        {
            var normalizedSkill = NormalizeSkill(skill);
            var similarSkills = new HashSet<string>();

            // Add direct synonyms
            foreach (var kvp in _skillSynonyms)
            {
                if (kvp.Key.Equals(normalizedSkill, StringComparison.OrdinalIgnoreCase) ||
                    kvp.Value.Any(s => s.Equals(skill, StringComparison.OrdinalIgnoreCase)))
                {
                    similarSkills.Add(kvp.Key);
                    similarSkills.UnionWith(kvp.Value.Take(topN));
                }
            }

            // Add skills from database that contain the search term
            var allSkills = await _unitOfWork.Skills.GetAllAsync();
            var matchingSkills = allSkills
                .Where(s => s.SkillName.Contains(skill, StringComparison.OrdinalIgnoreCase) ||
                           skill.Contains(s.SkillName, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.SkillName)
                .Take(topN);

            similarSkills.UnionWith(matchingSkills);

            return similarSkills.Take(topN).ToList();
        }

        public Task<double> CalculateSemanticSimilarityAsync(string skill1, string skill2)
        {
            var norm1 = NormalizeSkill(skill1);
            var norm2 = NormalizeSkill(skill2);

            // Exact match
            if (norm1.Equals(norm2, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(1.0);

            // Check if they're synonyms
            foreach (var kvp in _skillSynonyms)
            {
                var allVariants = new List<string> { kvp.Key };
                allVariants.AddRange(kvp.Value);

                if (allVariants.Contains(skill1, StringComparer.OrdinalIgnoreCase) &&
                    allVariants.Contains(skill2, StringComparer.OrdinalIgnoreCase))
                {
                    return Task.FromResult(0.9);
                }
            }

            // Partial match (contains)
            if (skill1.Contains(skill2, StringComparison.OrdinalIgnoreCase) ||
                skill2.Contains(skill1, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(0.7);
            }

            // Calculate Levenshtein distance similarity
            var distance = LevenshteinDistance(norm1, norm2);
            var maxLength = Math.Max(norm1.Length, norm2.Length);
            var similarity = 1.0 - ((double)distance / Math.Max(1, maxLength));

            return Task.FromResult(Math.Max(0, similarity));
        }

        // Helper: Normalize skill name using semantic mapping
        private string NormalizeSkill(string skill)
        {
            foreach (var kvp in _skillSynonyms)
            {
                if (kvp.Key.Equals(skill, StringComparison.OrdinalIgnoreCase) ||
                    kvp.Value.Any(s => s.Equals(skill, StringComparison.OrdinalIgnoreCase)))
                {
                    return kvp.Key;
                }
            }
            return skill;
        }

        // Helper: Normalize all student skills
        private List<Student> NormalizeStudentSkills(List<Student> students)
        {
            foreach (var student in students)
            {
                foreach (var studentSkill in student.StudentSkills)
                {
                    var normalized = NormalizeSkill(studentSkill.Skill.SkillName);
                    if (normalized != studentSkill.Skill.SkillName)
                    {
                        _logger.LogDebug("Normalized skill '{Original}' to '{Normalized}'",
                            studentSkill.Skill.SkillName, normalized);
                    }
                }
            }
            return students;
        }

        // Helper: Create feature vectors for ML
        private List<StudentFeatures> CreateFeatureVectors(List<Student> students)
        {
            // Get all unique normalized skills
            var allSkills = students
                .SelectMany(s => s.StudentSkills.Select(ss => NormalizeSkill(ss.Skill.SkillName)))
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            _logger.LogInformation("Creating feature vectors with {SkillCount} unique skills", allSkills.Count);

            if (allSkills.Count == 0)
                return new List<StudentFeatures>();

            var features = new List<StudentFeatures>();

            foreach (var student in students)
            {
                var studentSkills = student.StudentSkills
                    .Select(ss => NormalizeSkill(ss.Skill.SkillName))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // Create binary vector (1 if skill present, 0 if not)
                var vector = allSkills
                    .Select(skill => studentSkills.Contains(skill) ? 1.0f : 0.0f)
                    .ToArray();

                features.Add(new StudentFeatures
                {
                    StudentId = student.StudentId,
                    SkillVector = vector,
                    SkillCount = studentSkills.Count,
                    ExperienceCount = student.Experiences?.Count ?? 0
                });
            }

            return features;
        }

        // Helper: DBSCAN implementation (unchanged)
        private List<DBSCANResult> PerformDBSCAN(List<StudentFeatures> features, double epsilon, int minPoints)
        {
            var results = features.Select((f, i) => new DBSCANResult
            {
                Index = i,
                ClusterId = -1, // -1 means noise/unassigned
                IsCore = false,
                IsVisited = false
            }).ToList();

            int currentClusterId = 0;

            for (int i = 0; i < features.Count; i++)
            {
                if (results[i].IsVisited)
                    continue;

                results[i].IsVisited = true;

                var neighbors = GetNeighbors(features, i, epsilon);

                if (neighbors.Count < minPoints)
                {
                    results[i].ClusterId = -1; // Mark as noise
                }
                else
                {
                    results[i].IsCore = true;
                    ExpandCluster(features, results, i, neighbors, currentClusterId, epsilon, minPoints);
                    currentClusterId++;
                }
            }

            return results;
        }

        private List<int> GetNeighbors(List<StudentFeatures> features, int pointIndex, double epsilon)
        {
            var neighbors = new List<int>();
            var point = features[pointIndex].SkillVector;

            for (int i = 0; i < features.Count; i++)
            {
                if (i == pointIndex) continue;

                var distance = EuclideanDistance(point, features[i].SkillVector);
                if (distance <= epsilon)
                {
                    neighbors.Add(i);
                }
            }

            return neighbors;
        }

        private void ExpandCluster(List<StudentFeatures> features, List<DBSCANResult> results,
            int pointIndex, List<int> neighbors, int clusterId, double epsilon, int minPoints)
        {
            results[pointIndex].ClusterId = clusterId;

            var queue = new Queue<int>(neighbors);

            while (queue.Count > 0)
            {
                var currentIndex = queue.Dequeue();

                if (!results[currentIndex].IsVisited)
                {
                    results[currentIndex].IsVisited = true;
                    var currentNeighbors = GetNeighbors(features, currentIndex, epsilon);

                    if (currentNeighbors.Count >= minPoints)
                    {
                        results[currentIndex].IsCore = true;
                        foreach (var neighbor in currentNeighbors)
                        {
                            if (!results[neighbor].IsVisited)
                            {
                                queue.Enqueue(neighbor);
                            }
                        }
                    }
                }

                if (results[currentIndex].ClusterId == -1)
                {
                    results[currentIndex].ClusterId = clusterId;
                }
            }
        }

        // Helper: Calculate Euclidean distance
        private double EuclideanDistance(float[] vector1, float[] vector2)
        {
            double sum = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                var diff = vector1[i] - vector2[i];
                sum += diff * diff;
            }
            return Math.Sqrt(sum);
        }

        // Helper: Calculate Silhouette score
        private double CalculateSilhouetteScore(float[] vector, uint clusterId,
            List<StudentFeatures> allFeatures, List<ClusterPrediction> predictions)
        {
            // Simplified silhouette calculation
            var sameCluster = predictions
                .Where(p => p.PredictedClusterId == clusterId)
                .ToList();

            if (sameCluster.Count <= 1)
                return 0;

            // Average distance to points in same cluster
            double avgIntraDistance = 0;
            int intraCount = 0;

            for (int i = 0; i < predictions.Count; i++)
            {
                if (predictions[i].PredictedClusterId == clusterId)
                {
                    avgIntraDistance += EuclideanDistance(vector, allFeatures[i].SkillVector);
                    intraCount++;
                }
            }

            if (intraCount > 1)
                avgIntraDistance /= (intraCount - 1);

            // Convert to 0-100 scale (inverted - lower distance = higher score)
            return Math.Max(0, 100 - (avgIntraDistance * 50));
        }

        // Helper: Levenshtein distance for string similarity
        private int LevenshteinDistance(string s1, string s2)
        {
            var d = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                d[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                d[0, j] = j;

            for (int j = 1; j <= s2.Length; j++)
            {
                for (int i = 1; i <= s1.Length; i++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(
                        d[i - 1, j] + 1,
                        d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s1.Length, s2.Length];
        }

        private async Task<ClusterDetailDto> GetClusterDetails(int clusterId)
        {
            var cluster = await _unitOfWork.Clusters.GetByIdAsync(clusterId);
            if (cluster == null)
                throw new InvalidOperationException("Cluster not found");

            return new ClusterDetailDto
            {
                Id = cluster.Id,
                ClusterName = cluster.ClusterName,
                Description = cluster.Description,
                CommonSkills = ExtractCommonSkills(cluster.Members.ToList()),
                Students = cluster.Members.Select(m => new ClusterStudentDto
                {
                    StudentId = m.Student.Id,
                    StudentNo = m.Student.StudentId,
                    Name = m.Student.Name,
                    Email = m.Student.Email,
                    Skills = m.Student.StudentSkills.Select(ss => ss.Skill.SkillName).ToList(),
                    SimilarityScore = m.SimilarityScore
                }).ToList(),
                CreatedDate = cluster.CreatedDate
            };
        }

        private List<string> ExtractCommonSkills(List<ClusterMember> members)
        {
            if (members.Count == 0) return new List<string>();

            return members
                .SelectMany(m => m.MatchingSkills.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .GroupBy(s => s.Trim())
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();
        }

        private string GetCurrentUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                    return userId;

                var userName = httpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(userName))
                    return userName;
            }
            return "System";
        }
    }

    // ML.NET Data Models
    public class StudentClusterData
    {
        // Do not hardcode vector length; ML.NET will infer from provided feature arrays.
        public float[] Features { get; set; } = Array.Empty<float>();
    }

    public class ClusterPrediction
    {
        [ColumnName("PredictedLabel")]
        public uint PredictedClusterId { get; set; }

        [ColumnName("Score")]
        public float[] Distances { get; set; } = Array.Empty<float>();
    }

    public class StudentFeatures
    {
        public string StudentId { get; set; } = string.Empty;
        public float[] SkillVector { get; set; } = Array.Empty<float>();
        public int SkillCount { get; set; }
        public int ExperienceCount { get; set; }
    }

    public class DBSCANResult
    {
        public int Index { get; set; }
        public int ClusterId { get; set; }
        public bool IsCore { get; set; }
        public bool IsVisited { get; set; }
    }
}
