﻿using WMS_Application.Models;

namespace WMS_Application.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<List<TblUser>> GetAllEmployees(int id);
        List<TblUser> GetEmployeeReports(int userId);
    }
}
