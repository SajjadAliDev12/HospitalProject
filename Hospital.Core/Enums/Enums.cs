using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.Enums
{
    public enum enGender { Male,Female};
    public enum enShiftType { Morning,Night};
    public enum enJobStatus { Continuous, OnVacation , DisContinue , Retired , Departed };
    public enum enLeaveType { Hourly , SickLeave , Motherhood , Normal , AfterBirth , Yearly , WithOutSalary};
    public enum enCertificate { HighSchool , institute ,  Collage , Master , PHD , Prof};
    public enum enRole { Admin , Manager , User };
    public enum enAuditType {Add , Edit , Delete , Transfer , Leave , Absent};
    public enum enMorningShifts { SaturdayGroup, ThursdayGroup }
}
