using System;
using System.Collections.Generic;

namespace PRN232_FA25_Assignment_G7.Repositories.Entities;

public class DuplicateGroup
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public decimal SimilarityScore { get; set; }
    public DateTime CreatedAt { get; set; }

    public Exam? Exam { get; set; }
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}