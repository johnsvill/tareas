﻿using Microsoft.AspNetCore.Identity;

namespace Tareas.Entidades
{
    public class Tarea
    {
        public int Id { get; set; }

        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        public int Orden { get; set; }

        public DateTime FechaCreacion { get; set; }

        public string UsuarioCreacionId { get; set; }
        public IdentityUser UsuarioCreacion { get; set; }

        public List<Paso> Pasos { get; set; }

        public List<ArchivoAdjunto> ArchivoAdjuntos { get; set; }
    }
}
