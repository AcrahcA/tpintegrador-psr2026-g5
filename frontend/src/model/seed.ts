import type { DB } from './types.js';
export function createInitialData():DB {
return {
    version:2,
    refugio:{nombre:'Patitas · Refugio de mascotas',capacidadMaxima:18},
    mascotas:[
      {id:1,nombre:'Miel',especie:'Perro',raza:'Mestiza',edadAproximada:2,sexo:'Hembra',tamano:'Mediano',estado:'DisponibleAdopcion',cuidadorId:1,fechaIngreso:'2026-09-19',foto:'pet-maple.jpg'},
      {id:2,nombre:'Luna',especie:'Gato',raza:'Europeo',edadAproximada:1,sexo:'Hembra',tamano:'Pequenio',estado:'DisponibleAdopcion',cuidadorId:2,fechaIngreso:'2026-10-02',foto:'pet-luna.jpg'},
      {id:3,nombre:'Bruno',especie:'Perro',raza:'Mestizo',edadAproximada:4,sexo:'Macho',tamano:'Grande',estado:'EnTratamiento',cuidadorId:1,fechaIngreso:'2026-10-12',foto:'pet-otis.jpg'},
      {id:4,nombre:'Nube',especie:'Gato',raza:'Mestiza',edadAproximada:2,sexo:'Hembra',tamano:'Pequenio',estado:'EnRecuperacion',cuidadorId:3,fechaIngreso:'2026-10-05',foto:'pet-olive.jpg'},
      {id:5,nombre:'Coco',especie:'Perro',raza:'Mestizo',edadAproximada:3,sexo:'Macho',tamano:'Mediano',estado:'DisponibleAdopcion',cuidadorId:1,fechaIngreso:'2026-09-08',foto:'pet-biscuit.jpg'},
      {id:6,nombre:'Oliver',especie:'Gato',raza:'Mestizo',edadAproximada:5,sexo:'Macho',tamano:'Pequenio',estado:'DisponibleAdopcion',cuidadorId:2,fechaIngreso:'2026-10-16',foto:'pet-mochi.jpg'},
      {id:7,nombre:'Pipa',especie:'Perro',raza:'Mestiza',edadAproximada:1,sexo:'Hembra',tamano:'Pequenio',estado:'Reservada',cuidadorId:3,fechaIngreso:'2026-09-22',foto:'pet-chip.jpg'},
      {id:8,nombre:'Simón',especie:'Perro',raza:'Mestizo',edadAproximada:3,sexo:'Macho',tamano:'Grande',estado:'Adoptada',cuidadorId:null,fechaIngreso:'2026-08-10',foto:'pet-walter.jpg'},
      {id:9,nombre:'Mora',especie:'Gato',raza:'Mestiza',edadAproximada:2,sexo:'Hembra',tamano:'Pequenio',estado:'DisponibleAdopcion',cuidadorId:2,fechaIngreso:'2026-10-20',foto:'pet-saffron.jpg'},
      {id:10,nombre:'Toby',especie:'Perro',raza:'Mestizo',edadAproximada:6,sexo:'Macho',tamano:'Mediano',estado:'Adoptada',cuidadorId:null,fechaIngreso:'2026-07-15',foto:'pet-pepper.jpg'},
      {id:11,nombre:'Milo',especie:'Perro',raza:'Mestizo',edadAproximada:2,sexo:'Macho',tamano:'Mediano',estado:'Ingresada',cuidadorId:3,fechaIngreso:'2026-11-02',foto:'pet-nova.jpg'},
      {id:12,nombre:'Kiara',especie:'Gato',raza:'Mestiza',edadAproximada:3,sexo:'Hembra',tamano:'Pequenio',estado:'DisponibleAdopcion',cuidadorId:1,fechaIngreso:'2026-10-18',foto:'pet-juniper.jpg'}
    ],
    cuidadores:[{id:1,nombre:'Sofía Gómez',especializacion:'Cuidados generales',capacidadMaxima:4},{id:2,nombre:'Martín Pérez',especializacion:'Felinos',capacidadMaxima:4},{id:3,nombre:'Camila Torres',especializacion:'Rehabilitación',capacidadMaxima:4}],
    avisos:[
      {id:1,descripcion:'Perro lastimado cerca de una plaza',tipoAnimal:'Perro',contactoNombre:'Ana López',contactoTelefono:'11 5555-1234',latitud:-34.651,longitud:-58.479,estadoAparente:'Herido',nivelUrgencia:'Alta',estado:'Reportado',fechaAviso:'2026-11-06',mascotaGeneradaId:null},
      {id:2,descripcion:'Dos gatitos vistos cerca de una estación',tipoAnimal:'Gato',contactoNombre:'Nicolás R.',contactoTelefono:'11 5555-4567',latitud:-34.636,longitud:-58.462,estadoAparente:'Sano',nivelUrgencia:'Media',estado:'EnEvaluacion',fechaAviso:'2026-11-05',mascotaGeneradaId:null},
      {id:3,descripcion:'Animal en situación de abandono',tipoAnimal:'Perro',contactoNombre:'Valeria',contactoTelefono:'11 5555-7755',latitud:-34.645,longitud:-58.488,estadoAparente:'Desnutrido',nivelUrgencia:'Alta',estado:'Aceptado',fechaAviso:'2026-11-04',mascotaGeneradaId:null},
      {id:4,descripcion:'Perro encontrado en un baldío',tipoAnimal:'Perro',contactoNombre:'Diego F.',contactoTelefono:'11 5555-8899',latitud:-34.649,longitud:-58.472,estadoAparente:'Sano',nivelUrgencia:'Baja',estado:'Atendido',fechaAviso:'2026-11-02',mascotaGeneradaId:11}
    ],
    tratamientos:[
      {id:1,historialSanitarioId:1,mascotaId:3,tipo:'Curacion',descripcion:'Curación de pata delantera',estado:'EnCurso',fechaInicio:'2026-10-13',fechaFin:null},
      {id:2,historialSanitarioId:2,mascotaId:4,tipo:'Vacunacion',descripcion:'Esquema de vacunación',estado:'Finalizado',fechaInicio:'2026-10-06',fechaFin:'2026-11-01'},
      {id:3,historialSanitarioId:3,mascotaId:11,tipo:'ControlVeterinario',descripcion:'Control general de ingreso',estado:'Pendiente',fechaInicio:'2026-11-02',fechaFin:null},
      {id:4,historialSanitarioId:4,mascotaId:7,tipo:'Desparasitacion',descripcion:'Desparasitación de rutina',estado:'Finalizado',fechaInicio:'2026-10-10',fechaFin:'2026-10-12'}
    ],
    historiales:[{id:1,mascotaId:3,observaciones:'Requiere controles periódicos.'},{id:2,mascotaId:4,observaciones:'Vacunación al día.'},{id:3,mascotaId:11,observaciones:'Control de ingreso pendiente.'},{id:4,mascotaId:7,observaciones:'Desparasitación completa.'},{id:5,mascotaId:8,observaciones:'Historial conservado tras la adopción.'},{id:6,mascotaId:10,observaciones:'Historial conservado tras la adopción.'}],
    adoptantes:[{id:1,nombre:'Julieta Fernández',dni:'40000001',telefono:'11 5555-0001',direccion:''},{id:2,nombre:'Lucas Ramírez',dni:'40000002',telefono:'11 5555-0002',direccion:''},{id:3,nombre:'Paula Díaz',dni:'40000003',telefono:'11 5555-0003',direccion:''},{id:4,nombre:'Brenda Soto',dni:'40000004',telefono:'11 5555-0004',direccion:''},{id:5,nombre:'Federico Silva',dni:'40000005',telefono:'11 5555-0005',direccion:''}],
    solicitudes:[
      {id:1,adoptanteId:1,mascotaId:1,estado:'Pendiente',fechaSolicitud:'2026-11-06'},
      {id:2,adoptanteId:2,mascotaId:5,estado:'EnEvaluacion',fechaSolicitud:'2026-11-05'},
      {id:3,adoptanteId:3,mascotaId:8,estado:'Aprobada',fechaSolicitud:'2026-10-28'},
      {id:4,adoptanteId:4,mascotaId:2,estado:'Pendiente',fechaSolicitud:'2026-11-07'},
      {id:5,adoptanteId:5,mascotaId:10,estado:'Aprobada',fechaSolicitud:'2026-10-10'}
    ],
    adopciones:[{id:1,solicitudAdopcionId:3,fechaAdopcion:'2026-11-01',observaciones:'Adopción concretada.'},{id:2,solicitudAdopcionId:5,fechaAdopcion:'2026-10-15',observaciones:'Adopción concretada.'}]
  };
}
