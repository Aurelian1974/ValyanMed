using Shared.DTOs;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Client.Models
{
    public static class MappingExtensions
    {
        public static PersoanaDTO ToDTO(this PersoanaModel model)
        {
            return new PersoanaDTO
            {
                Id = model.Id,
                Guid = model.Guid,
                Nume = model.Nume,
                Prenume = model.Prenume,
                Judet = model.Judet,
                Localitate = model.Localitate,
                Strada = model.Strada,
                NumarStrada = model.NumarStrada,
                CodPostal = model.CodPostal,
                PozitieOrganizatie = model.PozitieOrganizatie,
                DataNasterii = model.DataNasterii,
                DataCreare = model.DataCreare,
                DataModificare = model.DataModificare,
                CNP = model.CNP,
                TipActIdentitate = model.TipActIdentitate,
                SerieActIdentitate = model.SerieActIdentitate,
                NumarActIdentitate = model.NumarActIdentitate,
                StareCivila = model.StareCivila,
                Gen = model.Gen,
                Specialitate = model.Specialitate,
                Departament = model.Departament,
                DataAngajarii = model.DataAngajarii ?? DateTime.Today,
                Status = model.Status
            };
        }

        public static List<PersoanaDTO> ToDTO(this List<PersoanaModel> models)
        {
            return models.Select(m => m.ToDTO()).ToList();
        }

        public static CreatePersoanaDTO ToCreateDto(this PersoanaModel model)
        {
            return new CreatePersoanaDTO
            {
                Nume = model.Nume,
                Prenume = model.Prenume,
                Judet = model.Judet,
                Localitate = model.Localitate,
                Strada = model.Strada,
                NumarStrada = model.NumarStrada,
                CodPostal = model.CodPostal,
                PozitieOrganizatie = model.PozitieOrganizatie,
                DataNasterii = model.DataNasterii,
                CNP = model.CNP,
                TipActIdentitate = model.TipActIdentitate,
                SerieActIdentitate = model.SerieActIdentitate,
                NumarActIdentitate = model.NumarActIdentitate,
                StareCivila = model.StareCivila,
                Gen = model.Gen,
                Specialitate = model.Specialitate,
                Departament = model.Departament,
                DataAngajarii = model.DataAngajarii,
                Status = model.Status
            };
        }

        public static UpdatePersoanaDTO ToUpdateDto(this PersoanaModel model)
        {
            return new UpdatePersoanaDTO
            {
                Id = model.Id,
                Nume = model.Nume,
                Prenume = model.Prenume,
                Judet = model.Judet,
                Localitate = model.Localitate,
                Strada = model.Strada,
                NumarStrada = model.NumarStrada,
                CodPostal = model.CodPostal,
                PozitieOrganizatie = model.PozitieOrganizatie,
                DataNasterii = model.DataNasterii,
                CNP = model.CNP,
                TipActIdentitate = model.TipActIdentitate,
                SerieActIdentitate = model.SerieActIdentitate,
                NumarActIdentitate = model.NumarActIdentitate,
                StareCivila = model.StareCivila,
                Gen = model.Gen,
                Specialitate = model.Specialitate,
                Departament = model.Departament,
                DataAngajarii = model.DataAngajarii,
                Status = model.Status
            };
        }
    }
}