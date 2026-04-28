using HTT.BlazorWasm.App.Contracts;

namespace HTT.BlazorWasm.App.Services
{
    /// <summary>
    /// Mock implementation of the permission service.
    /// In production, this should be integrated with Identity/Auth system.
    /// </summary>
    internal sealed class PermissionService : IPermissionService
    {
        public Task<bool> HasPermissionAsync(string? permission)
        {
            // For showcase purposes, assume all permissions are granted.
            return Task.FromResult(true);
        }
    }
}
