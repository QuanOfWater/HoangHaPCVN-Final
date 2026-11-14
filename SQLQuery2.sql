USE HoangHaPCVN;
GO

-- Thêm cột lưu biểu tượng Font Awesome cho danh mục (cho phép để trống)
ALTER TABLE Category ADD CategoryIcon NVARCHAR(50) NULL;
GO

-- Gán biểu tượng cho 8 danh mục, giống sidebar của teammate
UPDATE Category SET CategoryIcon = N'fa-solid fa-desktop'           WHERE CategoryName LIKE N'%PC Giá Rẻ%';
UPDATE Category SET CategoryIcon = N'fa-solid fa-microchip'         WHERE CategoryName LIKE N'%CPU%';
UPDATE Category SET CategoryIcon = N'fa-solid fa-gamepad'           WHERE CategoryName LIKE N'%VGA%';
UPDATE Category SET CategoryIcon = N'fa-solid fa-screwdriver-wrench' WHERE CategoryName LIKE N'%Linh Kiện%';
UPDATE Category SET CategoryIcon = N'fa-solid fa-tv'                WHERE CategoryName LIKE N'%Màn Hình%';
UPDATE Category SET CategoryIcon = N'fa-solid fa-fan'               WHERE CategoryName LIKE N'%Tản Nhiệt%';
UPDATE Category SET CategoryIcon = N'fa-solid fa-computer-mouse'    WHERE CategoryName LIKE N'%Gaming Gear%';
UPDATE Category SET CategoryIcon = N'fa-solidb fa-wifi'              WHERE CategoryName LIKE N'%Thiết Bị Mạng%';

-- Kiểm tra
SELECT CategoryID, CategoryName, CategoryIcon FROM Category;