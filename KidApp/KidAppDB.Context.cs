﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KidApp
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class KidAppEntities : DbContext
    {
        public KidAppEntities()
            : base("name=KidAppEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<EngResult> EngResults { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<VieResult> VieResults { get; set; }
    }
}
