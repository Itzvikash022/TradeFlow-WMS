﻿using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface ISidebarRepository
    {
        Task<List<SidebarModel>> GetTabsByRoleIdAsync(int roleId);
        TblUser GetUserById(int userId);
        TblUser GetCompanyById(int compId);

    }
}
