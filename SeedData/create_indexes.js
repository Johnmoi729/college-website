var db = db.getSiblingDB('CollegeDB'); 
db.Students.createIndex({'studentId': 1}, {unique: true}); 
db.Students.createIndex({'email': 1}); 
db.Students.createIndex({'departmentId': 1}); 
db.Students.createIndex({'admissionStatus': 1}); 
 
db.Courses.createIndex({'courseCode': 1}, {unique: true}); 
db.Courses.createIndex({'departmentId': 1}); 
db.Courses.createIndex({'facultyId': 1}); 
 
db.Faculty.createIndex({'facultyId': 1}, {unique: true}); 
db.Faculty.createIndex({'email': 1}, {unique: true}); 
db.Faculty.createIndex({'departmentId': 1}); 
 
db.Departments.createIndex({'departmentCode': 1}, {unique: true}); 
 
db.Admins.createIndex({'username': 1}, {unique: true}); 
db.Admins.createIndex({'email': 1}, {unique: true}); 
 
db.Feedback.createIndex({'email': 1}); 
db.Feedback.createIndex({'isResolved': 1}); 
db.Feedback.createIndex({'submissionDate': -1}); 
