using Microsoft.EntityFrameworkCore;
using Vulindlela.Clearance.Api.Business.Interfaces;
using Vulindlela.Clearance.Api.Data.Entities;

namespace Vulindlela.Clearance.Api.Data.Repositories
{
    public sealed class SubmissionRepository : ISubmissionRepository
    {
        private readonly ClearanceDbContext _db;
        public SubmissionRepository(ClearanceDbContext db) => _db = db;

        public Task<Submission?> GetByIdAsync(int id, CancellationToken ct) =>
            _db.Submissions
               .Include(s => s.Documents)
               .Include(s => s.ClearanceStatus)
                   .ThenInclude(c => c.MissingDocuments)
               .FirstOrDefaultAsync(s => s.Id == id, ct);

        public void Add(Submission submission) => _db.Submissions.Add(submission);
    }
}
