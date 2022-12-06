using MagicEastern.ADOExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOracle.Core
{
    
    [DBTable("employees")]
    class Employee
    {
        [DBColumn] public int EMPLOYEE_ID { get; set; }
        [DBColumn] public string FIRST_NAME { get; set; }
        [DBColumn] public string LAST_NAME { get; set; }
        [DBColumn] public string EMAIL { get; set; }
        [DBColumn] public string PHONE_NUMBER { get; set; }
        [DBColumn] public DateTime HIRE_DATE { get; set; }
        [DBColumn] public string JOB_ID { get; set; }
        [DBColumn] public decimal? SALARY { get; set; }
        [DBColumn] public decimal? COMMISSION_PCT { get; set; }
        [DBColumn] public int? MANAGER_ID { get; set; }
        [DBColumn] public int? DEPARTMENT_ID { get; set; }
    }
    
    //[DBTable("employees")]
    //class Employee
    //{
    //    [DBColumn] public int EMPLOYEE_ID { get; set; }
    //    //[DBColumn] public string FIRST_NAME { get; set; }
    //    [DBColumn] public string LAST_NAME { get; set; }
    //    //[DBColumn] public string EMAIL { get; set; }
    //    //[DBColumn] public DateTime HIRE_DATE { get; set; }
    //    //[DBColumn] public string JOB_ID { get; set; }
  
    //}
}
