using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LearnDotNetCore.Utilities
{
    public class ValidEmailDomainAttribute : ValidationAttribute
    {
        private string _allowedDomain;
        public ValidEmailDomainAttribute(string allowedDomain)
        {
            _allowedDomain = allowedDomain;
        }

        public override bool IsValid(object value)
        {
            string email = value.ToString();
            string[] emailArray = email.Split('@');
            return emailArray[1] == _allowedDomain;
        }
    }
}
