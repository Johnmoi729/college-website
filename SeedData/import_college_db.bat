@echo off
echo College Website MongoDB Import Script
echo ===================================
echo.

rem Set MongoDB connection parameters
set MONGO_HOST=localhost
set MONGO_PORT=27017
set DB_NAME=CollegeDB

echo Connecting to MongoDB at %MONGO_HOST%:%MONGO_PORT%...
echo.

rem Import collections
echo Importing Departments...
mongoimport --host %MONGO_HOST% --port %MONGO_PORT% --db %DB_NAME% --collection Departments --drop --file departments.json --jsonArray

echo Importing Faculty...
mongoimport --host %MONGO_HOST% --port %MONGO_PORT% --db %DB_NAME% --collection Faculty --drop --file faculty.json --jsonArray

echo Importing Courses...
mongoimport --host %MONGO_HOST% --port %MONGO_PORT% --db %DB_NAME% --collection Courses --drop --file courses.json --jsonArray

echo Importing Students...
mongoimport --host %MONGO_HOST% --port %MONGO_PORT% --db %DB_NAME% --collection Students --drop --file students.json --jsonArray

echo Importing Admins...
mongoimport --host %MONGO_HOST% --port %MONGO_PORT% --db %DB_NAME% --collection Admins --drop --file admins.json --jsonArray

echo Importing Feedback...
mongoimport --host %MONGO_HOST% --port %MONGO_PORT% --db %DB_NAME% --collection Feedback --drop --file feedback.json --jsonArray

echo.
echo Creating indexes for better performance...

rem Create indexes script
echo var db = db.getSiblingDB('%DB_NAME%'); > create_indexes.js
echo db.Students.createIndex({'studentId': 1}, {unique: true}); >> create_indexes.js
echo db.Students.createIndex({'email': 1}); >> create_indexes.js
echo db.Students.createIndex({'departmentId': 1}); >> create_indexes.js
echo db.Students.createIndex({'admissionStatus': 1}); >> create_indexes.js
echo. >> create_indexes.js
echo db.Courses.createIndex({'courseCode': 1}, {unique: true}); >> create_indexes.js
echo db.Courses.createIndex({'departmentId': 1}); >> create_indexes.js
echo db.Courses.createIndex({'facultyId': 1}); >> create_indexes.js
echo. >> create_indexes.js
echo db.Faculty.createIndex({'facultyId': 1}, {unique: true}); >> create_indexes.js
echo db.Faculty.createIndex({'email': 1}, {unique: true}); >> create_indexes.js
echo db.Faculty.createIndex({'departmentId': 1}); >> create_indexes.js
echo. >> create_indexes.js
echo db.Departments.createIndex({'departmentCode': 1}, {unique: true}); >> create_indexes.js
echo. >> create_indexes.js
echo db.Admins.createIndex({'username': 1}, {unique: true}); >> create_indexes.js
echo db.Admins.createIndex({'email': 1}, {unique: true}); >> create_indexes.js
echo. >> create_indexes.js
echo db.Feedback.createIndex({'email': 1}); >> create_indexes.js
echo db.Feedback.createIndex({'isResolved': 1}); >> create_indexes.js
echo db.Feedback.createIndex({'submissionDate': -1}); >> create_indexes.js

rem Run the index creation script
mongo < create_indexes.js

echo.
echo Database import completed successfully!
echo College Website Database is now ready for use.
echo.
echo Collection statistics:
echo -------------------
mongo %DB_NAME% --eval "db.Students.count()" --quiet
echo Students imported.
mongo %DB_NAME% --eval "db.Faculty.count()" --quiet
echo Faculty members imported.
mongo %DB_NAME% --eval "db.Courses.count()" --quiet
echo Courses imported.
mongo %DB_NAME% --eval "db.Departments.count()" --quiet
echo Departments imported.
mongo %DB_NAME% --eval "db.Admins.count()" --quiet
echo Admins imported.
mongo %DB_NAME% --eval "db.Feedback.count()" --quiet
echo Feedback items imported.
echo.
echo Database is now ready for your College Website application!
pause