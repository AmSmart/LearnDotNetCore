using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LearnDotNetCore.Models
{
    public class MockEmployeeRepository : IEmployeeRepository
    {
        private List<Employee> _employees;

        public MockEmployeeRepository()
        {
            _employees = new List<Employee>()
            {
               new Employee(){Name = "Smart", Id = 1, Email = "smart@smart.com", Department = Department.Software },
               new Employee(){Name = "Emmanuel", Id = 2, Email = "smart@emma.com", Department = Department.Hardware},
               new Employee(){Name = "Ogbemg", Id = 3, Email = "ogbemg@smart.com", Department = Department.Marketing}
            };
        }

        public Employee AddEmployee(Employee employee)
        {
            employee.Id = _employees.Count() + 1;
            _employees.Add(employee);
            return employee;
        }

        public Employee DeleteEmployee(int id)
        {
            var employeeToDelete = _employees.FirstOrDefault(x => x.Id == id);
            if (employeeToDelete != null)
            {
                _employees.Remove(employeeToDelete);
            }
            return employeeToDelete;
        }

        public Employee GetEmployee(int id)
        {
            return _employees.FirstOrDefault(x => x.Id == id);
        }

        public Employee UpdateEmployee(Employee updatedEmployee)
        {
            var employee = _employees.FirstOrDefault(x => x.Id == updatedEmployee.Id);
            if(employee != null)
            {
                employee.Name = updatedEmployee.Name;
                employee.Email = updatedEmployee.Email;
                employee.Department = updatedEmployee.Department;
            }
            
            return employee;
        }

        IEnumerable<Employee> IEmployeeRepository.GetAllEmployees()
        {
            return _employees;
        }
    }
}
