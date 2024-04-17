using System;
using System.Collections.Generic;

namespace BusinessObjects
{
    public partial class StudentGroup
    {
        public StudentGroup()
        {
            Students = new HashSet<Student>();
        }

        public int Id { get; set; }
        public string? Code { get; set; }
        public string? GroupName { get; set; }

        public virtual ICollection<Student> Students { get; set; }
    }
}
