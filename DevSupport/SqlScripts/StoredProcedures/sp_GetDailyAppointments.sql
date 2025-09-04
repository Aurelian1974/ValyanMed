-- Procedur? pentru program?ri zilnice ale unui medic
CREATE OR ALTER PROCEDURE sp_GetDailyAppointments
    @MedicalStaffId UNIQUEIDENTIFIER,
    @Date DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        a.Id,
        a.AppointmentDate,
        a.Duration,
        a.Type,
        a.Status,
        a.Notes,
        p.FirstName + ' ' + p.LastName AS PatientName,
        p.CNP,
        p.Phone,
        DATEDIFF(YEAR, p.DateOfBirth, GETDATE()) AS PatientAge
    FROM Appointments a
    INNER JOIN Patients p ON a.PatientId = p.Id
    WHERE 
        a.MedicalStaffId = @MedicalStaffId
        AND CAST(a.AppointmentDate AS DATE) = @Date
    ORDER BY a.AppointmentDate;
END;