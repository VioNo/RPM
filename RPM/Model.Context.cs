﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RPM
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DistilleryRassvetBase : DbContext
    {
        public DistilleryRassvetBase()
            : base("name=DistilleryRassvetBase")
        {
        }
        private static DistilleryRassvetBase _context;
        public static DistilleryRassvetBase GetContext()
        {
            if (_context == null)
                _context = new DistilleryRassvetBase();
            return _context;
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Clients> Clients { get; set; }
        public virtual DbSet<Climate> Climate { get; set; }
        public virtual DbSet<Employees> Employees { get; set; }
        public virtual DbSet<Fermentation> Fermentation { get; set; }
        public virtual DbSet<GrapeVarieties> GrapeVarieties { get; set; }
        public virtual DbSet<GrowingConditions> GrowingConditions { get; set; }
        public virtual DbSet<JobTitles> JobTitles { get; set; }
        public virtual DbSet<Orders> Orders { get; set; }
        public virtual DbSet<Party> Party { get; set; }
        public virtual DbSet<Payment> Payment { get; set; }
        public virtual DbSet<PaymentsOrder> PaymentsOrder { get; set; }
        public virtual DbSet<Products> Products { get; set; }
        public virtual DbSet<Shipments> Shipments { get; set; }
        public virtual DbSet<Soil> Soil { get; set; }
        public virtual DbSet<Storage> Storage { get; set; }
        public virtual DbSet<StorageWine> StorageWine { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<Water> Water { get; set; }
        public virtual DbSet<Yield> Yield { get; set; }
    }
}
