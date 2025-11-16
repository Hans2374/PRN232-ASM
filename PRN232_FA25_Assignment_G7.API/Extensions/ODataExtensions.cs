using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using PRN232_FA25_Assignment_G7.Repositories.Entities;

namespace PRN232_FA25_Assignment_G7.API.Extensions;

public static class ODataExtensions
{
    public static IMvcBuilder AddODataConfiguration(this IMvcBuilder builder)
    {
        var modelBuilder = new ODataConventionModelBuilder();
        
        modelBuilder.EntitySet<Subject>("Subjects");
        modelBuilder.EntitySet<Semester>("Semesters");
        modelBuilder.EntitySet<Exam>("Exams");
        modelBuilder.EntitySet<Submission>("Submissions");
        modelBuilder.EntitySet<Violation>("Violations");
        modelBuilder.EntitySet<Examiner>("Examiners");
        modelBuilder.EntitySet<Rubric>("Rubrics");

        builder.AddOData(options =>
        {
            options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100);
            options.AddRouteComponents("odata", modelBuilder.GetEdmModel());
        });

        return builder;
    }
}
