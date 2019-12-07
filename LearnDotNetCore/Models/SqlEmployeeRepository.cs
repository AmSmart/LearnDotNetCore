using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LearnDotNetCore.Models
{
    public class SqlEmployeeRepository:IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public SqlEmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        public Employee AddEmployee(Employee employee)
        {
            _context.Employees.Add(employee);
            _context.SaveChanges();
            return employee;
        }

        public Employee DeleteEmployee(int id)
        {
            var employeeToDelete = _context.Employees.Find(id);
            if(employeeToDelete != null)
            {
                _context.Employees.Remove(employeeToDelete);
                _context.SaveChanges();
            }
            return employeeToDelete;
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            var employees =  _context.Employees;
            return employees;
        }

        public Employee GetEmployee(int id)
        {
            var employee = _context.Employees.Find(id);
            _context.Entry(employee).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return employee;
        }

        public Employee UpdateEmployee(Employee employeeUpdate)
        {
            var employee = _context.Employees.Attach(employeeUpdate);
            employee.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();
            return employeeUpdate;
        }
    }
}
