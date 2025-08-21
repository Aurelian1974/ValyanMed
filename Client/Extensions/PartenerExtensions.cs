using Client.Models;
using Shared.DTOs;

namespace Client.Extensions
{
    public static class PartenerExtensions
    {
        public static PartenerDTO ToDTO(this PartenerModel model)
        {
            return new PartenerDTO
            {
                PartenerId = model.PartenerId,
                PartenerGuid = model.PartenerGuid,
                CodIntern = model.CodIntern,
                Denumire = model.Denumire,
                CodFiscal = model.CodFiscal,
                Judet = model.Judet,
                Localitate = model.Localitate,
                Adresa = model.Adresa,
                DataCreare = model.DataCreare,
                DataActualizare = model.DataActualizare,
                UtilizatorCreare = model.UtilizatorCreare,
                UtilizatorActualizare = model.UtilizatorActualizare,
                Activ = model.Activ
            };
        }

        public static CreatePartenerDTO ToCreateDto(this PartenerModel model)
        {
            return new CreatePartenerDTO
            {
                CodIntern = model.CodIntern,
                Denumire = model.Denumire,
                CodFiscal = model.CodFiscal,
                Judet = model.Judet,
                Localitate = model.Localitate,
                Adresa = model.Adresa,
                UtilizatorCreare = model.UtilizatorCreare
            };
        }

        public static UpdatePartenerDTO ToUpdateDto(this PartenerModel model)
        {
            return new UpdatePartenerDTO
            {
                PartenerId = model.PartenerId,
                CodIntern = model.CodIntern,
                Denumire = model.Denumire,
                CodFiscal = model.CodFiscal,
                Judet = model.Judet,
                Localitate = model.Localitate,
                Adresa = model.Adresa,
                UtilizatorActualizare = model.UtilizatorActualizare
            };
        }

        public static PartenerModel ToModel(this PartenerDTO dto)
        {
            return new PartenerModel
            {
                PartenerId = dto.PartenerId,
                PartenerGuid = dto.PartenerGuid,
                CodIntern = dto.CodIntern,
                Denumire = dto.Denumire,
                CodFiscal = dto.CodFiscal,
                Judet = dto.Judet,
                Localitate = dto.Localitate,
                Adresa = dto.Adresa,
                DataCreare = dto.DataCreare,
                DataActualizare = dto.DataActualizare,
                UtilizatorCreare = dto.UtilizatorCreare,
                UtilizatorActualizare = dto.UtilizatorActualizare,
                Activ = dto.Activ
            };
        }

        public static List<PartenerModel> ToModelList(this IEnumerable<PartenerDTO> dtos)
        {
            return dtos.Select(dto => dto.ToModel()).ToList();
        }

        public static List<PartenerDTO> ToDTOList(this IEnumerable<PartenerModel> models)
        {
            return models.Select(model => model.ToDTO()).ToList();
        }
    }
}