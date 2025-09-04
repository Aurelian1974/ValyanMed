# API Documentation - ValyanMed

## Autentificare

Toate endpoint-urile (cu excep?ia login/register) necesit? autentificare JWT.

**Header necesar:**
```
Authorization: Bearer {token}
```

## Patients API

### GET /api/patients
Returneaz? lista tuturor pacien?ilor.

**Response:**
```json
[
  {
    "id": "guid",
    "cnp": "string",
    "firstName": "string",
    "lastName": "string",
    "dateOfBirth": "date",
    "age": "number",
    "gender": "string",
    "phone": "string",
    "email": "string",
    "address": "string",
    "createdAt": "datetime",
    "updatedAt": "datetime"
  }
]
```

### GET /api/patients/{id}
Returneaz? un pacient specific.

**Parameters:**
- `id` (guid): ID-ul pacientului

**Response:**
```json
{
  "id": "guid",
  "cnp": "string",
  "firstName": "string",
  "lastName": "string",
  "dateOfBirth": "date",
  "age": "number",
  "gender": "string",
  "phone": "string",
  "email": "string",
  "address": "string"
}
```

### POST /api/patients
Creeaz? un pacient nou.

**Request Body:**
```json
{
  "cnp": "string (required, 13 chars)",
  "firstName": "string (required)",
  "lastName": "string (required)",
  "dateOfBirth": "date (required)",
  "gender": "string (required)",
  "phone": "string (optional)",
  "email": "string (optional)",
  "address": "string (optional)"
}
```

**Response:** Status 201 + pacient creat

### PUT /api/patients/{id}
Actualizeaz? un pacient existent.

**Parameters:**
- `id` (guid): ID-ul pacientului

**Request Body:**
```json
{
  "firstName": "string",
  "lastName": "string",
  "phone": "string",
  "email": "string",
  "address": "string"
}
```

### DELETE /api/patients/{id}
?terge un pacient.

**Parameters:**
- `id` (guid): ID-ul pacientului

**Response:** Status 204 (No Content)

### GET /api/patients/search
Caut? pacien?i dup? diverse criterii.

**Query Parameters:**
- `searchTerm` (string): Termen de c?utare (nume, CNP, telefon, email)
- `ageFrom` (int): Vârsta minim?
- `ageTo` (int): Vârsta maxim?
- `gender` (string): Genul
- `pageNumber` (int): Num?rul paginii (default: 1)
- `pageSize` (int): Dimensiunea paginii (default: 20)

**Response:**
```json
{
  "data": [...], // array de pacien?i
  "totalCount": "number",
  "pageNumber": "number",
  "pageSize": "number",
  "totalPages": "number"
}
```

## Medical Staff API

### GET /api/medicalstaff
Returneaz? lista personalului medical.

### POST /api/medicalstaff
Creeaz? personal medical nou.

**Request Body:**
```json
{
  "firstName": "string (required)",
  "lastName": "string (required)",
  "position": "string (required)",
  "specialization": "string",
  "department": "string (required)",
  "licenseNumber": "string",
  "phone": "string",
  "email": "string",
  "isActive": "boolean"
}
```

## Appointments API

### GET /api/appointments
Returneaz? lista program?rilor.

**Query Parameters:**
- `date` (date): Filtrare dup? dat?
- `medicalStaffId` (guid): Filtrare dup? personal medical
- `patientId` (guid): Filtrare dup? pacient
- `status` (string): Filtrare dup? status

### POST /api/appointments
Creeaz? o programare nou?.

**Request Body:**
```json
{
  "patientId": "guid (required)",
  "medicalStaffId": "guid (required)",
  "appointmentDate": "datetime (required)",
  "duration": "number (minutes, default: 30)",
  "type": "string (required)",
  "notes": "string"
}
```

## Error Responses

Toate endpoint-urile pot returna urm?toarele coduri de eroare:

### 400 Bad Request
```json
{
  "errors": [
    "Error message 1",
    "Error message 2"
  ]
}
```

### 401 Unauthorized
```json
{
  "message": "Token invalid sau lips?"
}
```

### 403 Forbidden
```json
{
  "message": "Acces interzis"
}
```

### 404 Not Found
```json
{
  "message": "Resursa nu a fost g?sit?"
}
```

### 500 Internal Server Error
```json
{
  "message": "Eroare intern? de server"
}
```

## Rate Limiting

- **Limit? general?**: 100 requests/minute per IP
- **Limit? autentificare**: 5 requests/minute per IP pentru /auth endpoints

## Exemple cURL

### Autentificare
```bash
curl -X POST https://api.valyanmed.ro/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@valyanmed.ro","password":"password"}'
```

### Creare pacient
```bash
curl -X POST https://api.valyanmed.ro/api/patients \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "cnp": "1234567890123",
    "firstName": "Ion",
    "lastName": "Popescu",
    "dateOfBirth": "1990-01-01",
    "gender": "Masculin",
    "phone": "0721234567",
    "email": "ion.popescu@email.ro"
  }'
```

### C?utare pacien?i
```bash
curl -X GET "https://api.valyanmed.ro/api/patients/search?searchTerm=Popescu&pageSize=10" \
  -H "Authorization: Bearer {token}"
```