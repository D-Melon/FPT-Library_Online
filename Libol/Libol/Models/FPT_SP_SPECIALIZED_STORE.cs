//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Libol.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class FPT_SP_SPECIALIZED_STORE
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Subjects { get; set; }
        public int LibID { get; set; }
    
        public virtual HOLDING_LIBRARY HOLDING_LIBRARY { get; set; }
    }
}
