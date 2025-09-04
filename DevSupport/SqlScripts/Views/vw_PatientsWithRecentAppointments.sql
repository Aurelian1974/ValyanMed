-- View pentru raportul pacien?ilor cu program?ri recente
CREATE OR ALTER VIEW vw_PatientsWithRecentAppointments
AS
SELECT 
    p.Id,
    p.CNP,
    p.FirstName,
    p.LastName,
    p.DateOfBirth,
    DATEDIFF(YEAR, p.DateOfBirth, GETDATE()) AS Age,
    p.Gender,
    p.Phone,
    p.Email,
    COALESCE(a.LastAppointment, 'Niciodat?') AS LastAppointmentDate,
    COALESCE(ms.LastDoctor, 'N/A') AS LastDoctor,
    COALESCE(a.TotalAppointments, 0) AS TotalAppointments
FROM Patients p
LEFT JOIN (
    SELECT 
        PatientId,
        MAX(AppointmentDate) AS LastAppointment,
        COUNT(*) AS TotalAppointments
    FROM Appointments
    GROUP BY PatientId
) a ON p.Id = a.PatientId
LEFT JOIN (
    SELECT DISTINCT
        a.PatientId,
        FIRST_VALUE(ms.FirstName + ' ' + ms.LastName) 
        OVER (PARTITION BY a.PatientId ORDER BY a.AppointmentDate DESC) AS LastDoctor
    FROM Appointments a
    INNER JOIN MedicalStaff ms ON a.MedicalStaffId = ms.Id
) ms ON p.Id = ms.PatientId;