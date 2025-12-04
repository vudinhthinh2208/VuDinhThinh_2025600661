using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DangKyLamThem
{
    internal class DataUtils
    {
        string filePath = "DangKyLamThem.xml";
        XmlDocument doc = new XmlDocument();
        XmlElement root;
        public DataUtils()
        {
            if (!File.Exists(filePath))
            {
                root = doc.CreateElement("NgayLamViec");
                doc.AppendChild(root);
                doc.Save(filePath);
            }
            doc.Load(filePath);
            root = doc.DocumentElement;
        }

        public List<NhanVien> Show()
        {
            List<NhanVien> nhanViens = new List<NhanVien>();
            XmlNodeList ngayNodes = root.SelectNodes("NgayLamViec");
            foreach(XmlNode ngay in  ngayNodes)
            {
                XmlNodeList nvNodes = ngay.SelectNodes("NhanVien");
                foreach(XmlNode nv in nvNodes)
                {
                    NhanVien nhanVien = new NhanVien
                    {
                        ngayLamViec = ngay.SelectSingleNode("@Ngay").InnerText,
                        maNV = nv.SelectSingleNode("@Ma").InnerText,
                        loaiLamThem = nv.SelectSingleNode("LoaiLamThem").InnerText,
                        soGio = double.Parse(nv.SelectSingleNode("SoGio").InnerText),
                        trangthai = nv.SelectSingleNode("TrangThai").InnerText
                    };
                    nhanViens.Add(nhanVien);
                }
            }
            return nhanViens;
        }

        public bool CheckAdd(string ngayLamViec, string maNhanVien)
        {   
            XmlNode checkNode = root.SelectSingleNode(
                $"NgayLamViec[@Ngay='{ngayLamViec}']/NhanVien[@Ma='{maNhanVien}']"
            );

            return checkNode != null;
        }


        public void Add(NhanVien nhanVien)
        {
            XmlElement ngayNode = doc.CreateElement("NgayLamViec");

            XmlAttribute ngayAttr = doc.CreateAttribute("Ngay");
            ngayAttr.Value = nhanVien.ngayLamViec;
            ngayNode.Attributes.Append(ngayAttr);

            root.AppendChild(ngayNode);

            XmlElement nvNode = doc.CreateElement("NhanVien");
            XmlAttribute maAttr = doc.CreateAttribute("Ma");
            XmlElement loaiNode = doc.CreateElement("LoaiLamThem");
            XmlElement soGioNode = doc.CreateElement("SoGio");
            XmlElement trangThaiNode = doc.CreateElement("TrangThai");

            maAttr.Value = nhanVien.maNV;
            loaiNode.InnerText = nhanVien.loaiLamThem;
            soGioNode.InnerText = nhanVien.soGio.ToString();
            trangThaiNode.InnerText = nhanVien.trangthai;

            nvNode.Attributes.Append(maAttr);
            nvNode.AppendChild(loaiNode);
            nvNode.AppendChild(soGioNode);
            nvNode.AppendChild(trangThaiNode);

            ngayNode.AppendChild(nvNode);

            doc.Save(filePath);
        }

        public bool Update(string oldNgay, string oldMa, NhanVien nv)
        {
            // 1. Tìm node nhân viên cũ
            XmlNode oldNode = root.SelectSingleNode(
                $"NgayLamViec[@Ngay='{oldNgay}']/NhanVien[@Ma='{oldMa}']"
            );

            if (oldNode == null)
                return false;

            // 2. Kiểm tra xem Ngày + Mã mới có bị trùng với nhân viên khác không
            XmlNode checkDup = root.SelectSingleNode(
                $"NgayLamViec[@Ngay='{nv.ngayLamViec}']/NhanVien[@Ma='{nv.maNV}']"
            );

            // Nếu trùng và KHÔNG phải chính nó → báo lỗi
            if (checkDup != null && checkDup != oldNode)
                return false;  // báo trùng

            // 3. Nếu ngày thay đổi → di chuyển node
            XmlNode newNgayNode = root.SelectSingleNode($"NgayLamViec[@Ngay='{nv.ngayLamViec}']");

            // Nếu ngày mới chưa có → tạo mới
            if (newNgayNode == null)
            {
                newNgayNode = doc.CreateElement("NgayLamViec");
                XmlAttribute attr = doc.CreateAttribute("Ngay");
                attr.Value = nv.ngayLamViec;
                newNgayNode.Attributes.Append(attr);

                root.AppendChild(newNgayNode);
            }

            // Nếu đổi ngày → chuyển node nhân viên sang ngày mới
            if (oldNgay != nv.ngayLamViec)
            {
                oldNode.ParentNode.RemoveChild(oldNode);
                newNgayNode.AppendChild(oldNode);
            }

            // 4. Cập nhật mã NV (attribute @Ma)
            XmlAttribute maAttr = oldNode.Attributes["Ma"];
            maAttr.Value = nv.maNV;

            // 5. Cập nhật các node còn lại
            oldNode.SelectSingleNode("LoaiLamThem").InnerText = nv.loaiLamThem;
            oldNode.SelectSingleNode("SoGio").InnerText = nv.soGio.ToString();
            oldNode.SelectSingleNode("TrangThai").InnerText = nv.trangthai;

            // 6. Lưu file
            doc.Save(filePath);

            return true;
        }

        public bool Delete(string ngay, string ma)
        {
            // Tìm node nhân viên cần xóa
            XmlNode nvNode = root.SelectSingleNode(
                $"NgayLamViec[@Ngay='{ngay}']/NhanVien[@Ma='{ma}']"
            );

            if (nvNode == null)
                return false;  // không tìm thấy

            // Lấy node ngày làm việc
            XmlNode ngayNode = nvNode.ParentNode;

            // Xóa nhân viên
            ngayNode.RemoveChild(nvNode);

            // Nếu ngày không còn nhân viên nào → xóa luôn ngày
            if (ngayNode.SelectNodes("NhanVien").Count == 0)
            {
                root.RemoveChild(ngayNode);
            }

            // Lưu file
            doc.Save(filePath);

            return true;
        }


        public List<NhanVien> Search(string ngay, string ma)
        {
            List<NhanVien> result = new List<NhanVien>();

            // Tạo câu truy vấn động
            string query = "NgayLamViec";
            List<string> conditions = new List<string>();

            if (!string.IsNullOrEmpty(ngay))
                conditions.Add($"@Ngay='{ngay}'");

            if (conditions.Count > 0)
                query += "[" + string.Join(" and ", conditions) + "]";

            query += "/NhanVien";

            if (!string.IsNullOrEmpty(ma))
                query += $"[@Ma='{ma}']";

            // Chạy truy vấn
            XmlNodeList nodes = root.SelectNodes(query);

            foreach (XmlNode nvNode in nodes)
            {
                XmlNode ngayNode = nvNode.ParentNode;

                result.Add(new NhanVien
                {
                    ngayLamViec = ngayNode.Attributes["Ngay"].Value,
                    maNV = nvNode.Attributes["Ma"].Value,
                    loaiLamThem = nvNode.SelectSingleNode("LoaiLamThem").InnerText,
                    soGio = double.Parse(nvNode.SelectSingleNode("SoGio").InnerText),
                    trangthai = nvNode.SelectSingleNode("TrangThai").InnerText
                });
            }

            return result;
        }

    }
}
