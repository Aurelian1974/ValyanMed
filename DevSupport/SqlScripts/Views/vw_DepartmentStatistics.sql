-- View pentru statistici departamente
CREATE OR ALTER VIEW vw_DepartmentStatistics
AS
SELECT 
    ms.Department,
    COUNT(*) AS TotalStaff,
    COUNT(CASE WHEN ms.IsActive = 1 THEN 1 END) AS ActiveStaff,
    COUNT(CASE WHEN ms.IsActive = 0 THEN 1 END) AS InactiveStaff,
    COALESCE(a.TotalAppointments, 0) AS TotalAppointments,
    COALESCE(a.AppointmentsThisMonth, 0) AS AppointmentsThisMonth,
    COALESCE(a.AppointmentsToday, 0) AS AppointmentsToday
FROM MedicalStaff ms
LEFT JOIN (
    SELECT 
        ms.Department,
        COUNT(*) AS TotalAppointments,
        COUNT(CASE WHEN MONTH(a.AppointmentDate) = MONTH(GETDATE()) 
                   AND YEAR(a.AppointmentDate) = YEAR(GETDATE()) THEN 1 END) AS AppointmentsThisMonth,
        COUNT(CASE WHEN CAST(a.AppointmentDate AS DATE) = CAST(GETDATE() AS DATE) THEN 1 END) AS AppointmentsToday
    FROM Appointments a
    INNER JOIN MedicalStaff ms ON a.MedicalStaffId = ms.Id
    GROUP BY ms.Department
) a ON ms.Department = a.Department
GROUP BY ms.Department, a.TotalAppointments, a.AppointmentsThisMonth, a.AppointmentsToday;