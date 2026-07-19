namespace SamorodinkaTech.Fiducia.Domain.Enums;

public enum MeetingForm
{
    /// <summary>Очное заседание: совместное присутствие членов совета директоров.</summary>
    OCHN = 0,
    /// <summary>Заочное голосование: опросным путём без проведения заседания.</summary>
    ZAOCHN = 1,
    /// <summary>Смешанное: очное заседание + заочное голосование (п. 1 ст. 68 208-ФЗ).</summary>
    MIXED = 2
}
