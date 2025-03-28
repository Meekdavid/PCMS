using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.General
{
    /// <summary>
    /// Defines an interface for managing member context information, such as member ID, roles, and claims.
    /// </summary>
    public interface IMemberContextManager
    {
        /// <summary>
        /// Asynchronously retrieves the current member's ID.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, returning the member's ID as a string.</returns>
        Task<string> GetMemberId();

        /// <summary>
        /// Asynchronously retrieves the current member's roles.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, returning a list of role names as strings.</returns>
        Task<List<string>> GetMemberRoles();

        /// <summary>
        /// Asynchronously checks if the current member is in the specified role.
        /// </summary>
        /// <param name="role">The role to check.</param>
        /// <returns>A task representing the asynchronous operation, returning true if the member is in the role, otherwise false.</returns>
        Task<bool> IsInRole(string role);

        /// <summary>
        /// Asynchronously retrieves the current member's name.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, returning the member's name as a string.</returns>
        Task<string> GetMemberName();

        /// <summary>
        /// Asynchronously retrieves the current member's email address.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, returning the member's email address as a string.</returns>
        Task<string> GetMemberEmail();

        /// <summary>
        /// Asynchronously retrieves the current member's claims.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, returning a list of member claims.</returns>
        Task<List<Claim>> GetMemberClaims();
    }
}
