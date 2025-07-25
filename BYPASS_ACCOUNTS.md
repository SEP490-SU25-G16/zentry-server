# 🔐 ZENTRY BYPASS ACCOUNTS FOR TESTING

## 📋 Admin Account (✅ TESTED & WORKING)

**✅ BEST FOR FACE ID TESTING:**

```
📧 Email: admin@zentry.com
🔑 Password: User@123456
👤 Role: Admin
🆔 User ID: d60d8eb1-0cbe-4324-8757-180d123ac061
🎯 Status: ✅ LOGIN TESTED - WORKING
🔐 Login Endpoint: /api/auth/sign-in
```

## 📋 Alternative Test Accounts

### Manager Account:

```
📧 Email: manager@zentry.com
🔑 Password: User@123456
👤 Role: Manager
🆔 User ID: 7b84fc68-f26d-43fe-8cd2-a4ef02c83ca7
🎯 Status: Available (not tested)
```

### Student Accounts (Pick any):

**Student 1 (✅ TESTED):**

```
📧 Email: v.th.ngn.student481@sinhvien.edu.vn
🔑 Password: User@123456
👤 Role: Student
🆔 User ID: 770eef88-ca59-43eb-9a01-d97b2c604ed5
👨‍🎓 Name: Vũ Thị Ngân
🎯 Status: ✅ LOGIN TESTED - WORKING
```

**Student 2:**

```
📧 Email: ng.dnh.trng.student186@mail.student.com
🔑 Password: User@123456
👤 Role: Student
🆔 User ID: 7e143f8c-ac51-4747-9213-9fc34cf2b320
👨‍🎓 Name: Ngô Đình Trọng
```

**Student 3:**

```
📧 Email: cao.vn.khoa.student435@student.fpt.edu.vn
🔑 Password: User@123456
👤 Role: Student
🆔 User ID: 74ce1f62-b987-48b0-bb4b-5edfb9a4fc1e
👨‍🎓 Name: Cao Văn Khoa
```

### Lecturer Account:

```
📧 Email: ng.vn.trung.lecturer9@zentry.edu
🔑 Password: User@123456
👤 Role: Lecturer
🆔 User ID: 15d1b3c4-10af-411a-83e3-2fa420092e6f
👨‍🏫 Name: Ông Văn Trung
```

## 🚀 How to Use for Face ID Testing

### 1. Login to Android App:

```
1. Open Zentry Android app
2. Use RECOMMENDED account:
   📧 Email: admin@zentry.com
   🔑 Password: User@123456
3. Login successfully ✅
```

### 2. Test Face ID Registration:

```
1. Go to Settings → Register Face ID
2. Allow camera permission
3. Position face in camera
4. Complete registration
5. Verify success message
```

### 3. Test Face ID Verification:

```
1. Logout from app
2. Try to login with Face ID
3. Position face in camera
4. Verify successful login
```

### 4. Test Face ID Update:

```
1. Go to Settings → Update Face ID
2. Register new face data
3. Verify update success
4. Test login with new face data
```

## ✅ Verified Integration Status

### Server Side:

- ✅ **API Health**: http://localhost:8080/health - Working
- ✅ **Authentication**: /api/auth/sign-in - Working
- ✅ **Face ID APIs**: /api/faceid/\* - Working
- ✅ **Database**: PostgreSQL + pgvector - Working
- ✅ **Users**: 500+ seeded accounts ready

### Android App Side:

- ✅ **Network Config**: Updated to http://10.0.2.2:8080/
- ✅ **Permissions**: INTERNET, CAMERA - Added
- ✅ **Security Config**: HTTP localhost allowed
- ✅ **API Integration**: RetrofitClient configured

### Authentication Test Results:

```
✅ admin@zentry.com - Status 200 OK
✅ v.th.ngn.student481@sinhvien.edu.vn - Status 200 OK
✅ Token generation working
✅ UserInfo parsing working
```

## 🎯 Testing Strategy

### Full Flow Test:

1. **Registration**: Use admin account to register Face ID
2. **Verification**: Login using Face ID
3. **Update**: Change Face ID data
4. **Re-verification**: Login with updated Face ID

### Multi-User Test:

1. **Admin**: Register and test Face ID
2. **Student**: Register different person's Face ID
3. **Cross-verification**: Try admin face on student account (should fail)
4. **Proper verification**: Each face should only work with correct account

## 📱 Android App Configuration

### Network Setup:

- ✅ Base URL: `http://10.0.2.2:8080/` (Android emulator)
- ✅ For real device: Use PC's IP address like `http://192.168.1.XXX:8080/`
- ✅ HTTP traffic allowed via network security config

### Permissions Required:

- ✅ INTERNET permission
- ✅ CAMERA permission
- ✅ Network access to localhost

### API Response Format:

```json
{
  "Token": "eyJhbGciOiJIUzI1NiIs...",
  "UserInfo": {
    "Id": "d60d8eb1-0cbe-4324-8757-180d123ac061",
    "Email": "admin@zentry.com",
    "Role": "Admin"
  }
}
```

## 🐛 Troubleshooting

### If login fails:

1. ✅ Check server is running: `docker-compose ps`
2. ✅ Test API: `curl http://localhost:8080/health`
3. ✅ Verify network config in Android app
4. ✅ Use correct endpoint: `/api/auth/sign-in` (not `/api/auth/login`)

### If Face ID registration fails:

1. ✅ Check user exists in database
2. ✅ Verify camera permissions granted
3. ✅ Check API logs: `docker-compose logs zentry-api`
4. ✅ Ensure user ID matches (d60d8eb1-0cbe-4324-8757-180d123ac061 for admin)

### If Face ID verification fails:

1. ✅ Ensure same user as registration
2. ✅ Check face is clearly visible
3. ✅ Verify similarity threshold (default: 0.7)

## 🎉 Success Criteria

- ✅ **Login Success**: Can login with email/password
- ✅ **Registration Success**: Face ID registered without errors
- ✅ **Verification Success**: Can login using face recognition
- ✅ **Update Success**: Can update Face ID with new face data
- ✅ **Security Success**: Face ID only works for correct user

## 🚀 Ready for Production Testing

**Status: ✅ FULLY READY FOR ANDROID APP TESTING!**

**Next Action**: Build Android app and test Face ID functionality with admin account!
