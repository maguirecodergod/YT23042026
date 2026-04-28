namespace HTT.BlazorWasm.App.Contracts
{
    /// <summary>
    /// Enterprise-grade permission service interface.
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Checks if the current user has the specified permission.
        /// </summary>
        Task<bool> HasPermissionAsync(string? permission);
    }
}
