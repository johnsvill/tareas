using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tareas.Entidades;

namespace Tareas.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        public virtual DbSet<Tarea> Tareas { get; set; }

        public virtual DbSet<Paso> Pasos { get; set; }

        public virtual DbSet<ArchivoAdjunto> ArchivoAdjuntos { get; set; }        
    }
}
