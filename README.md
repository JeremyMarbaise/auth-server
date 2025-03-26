# auth-server
# **Authentication Server for Belgian eID**  
A lightweight authentication server that generates challenges and verifies signatures using the Belgian eID card system.  

---

## **📌 Features**  
✅ **Challenge Generation** – Securely generates a challenge for client authentication.  
✅ **Signature Verification** – Validates signed challenges using the Belgian eID card.  
✅ **In-Memory Storage** – Temporarily stores challenges in a `ConcurrentDictionary`.  
✅ **Simple API** – Easy-to-use endpoints for integration with frontend applications.  

---

## **🚀 Getting Started**  

### **Prerequisites**  
- [.NET 6.0+](https://dotnet.microsoft.com/download)  
- Belgian eID middleware installed  
- A client application that can sign challenges (e.g., a web frontend)  

### **Installation**  
1. Clone the repository:  
   ```sh
   git clone https://github.com/your-repo/authentication-server.git
   cd authentication-server
   ```
2. Run the server:  
   ```sh
   dotnet run
   ```
   The server will start at `https://localhost:7043`.  

---

## **🔌 API Endpoints**  

### **1. Get a Challenge**  
**Endpoint:**  
`GET /auth/challenge?clientId={clientId}`  

**Request:**  
```http
GET /auth/challenge?clientId=test-client-123 HTTP/1.1
Host: localhost:7043
```  

**Response:**  
```json
{
  "challenge": [. . .] //byte array
}
```  

### **2. Authenticate with Signature**  
**Endpoint:**  
`POST /auth/authenticate`  

**Request:**  
```http
POST /auth/authenticate HTTP/1.1
Content-Type: application/json

{
  "clientId": "test-client-123",
  "signature": [ ... ] // Byte array of the signed challenge
}
```  

**Response:**  
```json
{
  "authenticated": true
}
```  




## **🔒 Security Notes**  
⚠ **For Development Only**  
- The server uses an in-memory challenge store (`ConcurrentDictionary`).  
- In production, use a **persistent database**  
- Always enforce **HTTPS** in production.  

---

## **📂 Project Structure**  
```
auth-server/
├── Program.cs             # Main server setup
├── Endpoints/
│   └── EIDModule.cs       # Authentication endpoints
└── appsettings.json       # Configuration
```

---

## **🛠 Troubleshooting**  


### **Missing eID Middleware**  
Ensure the [Belgian eID middlewareis](https://github.com/JeremyMarbaise/eid-mw.git) installed and running.  


---

