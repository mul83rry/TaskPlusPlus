using TaskPlusPlus.API.DbContexts;
using System;
using System.Threading.Tasks;

namespace TaskPlusPlus.API.Services
{
    public partial class TaskPlusPlusRepository : ITaskPlusPlusRepository, IDisposable
    {
        private TaskPlusPlusContext context;

        public TaskPlusPlusRepository(TaskPlusPlusContext context) => this.context = context ?? throw new ArgumentNullException(nameof(context));

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || context == null) return;
            context.Dispose();
            context = null;
        }


        private async Task<bool> HasPermissions(Guid userId, Guid parentId, Permissions permissionType)
        {
            var boardId = await GetBoardIdAsync(parentId);
            var RoleAccess = await HasRoleAccess(boardId, userId, permissionType);
            var tagAccess = await HasTagAccess(boardId, userId, parentId);

            if (!RoleAccess || !tagAccess) return false;

            return true;
        }

    }
}
