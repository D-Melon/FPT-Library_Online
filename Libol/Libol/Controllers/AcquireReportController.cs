﻿using Libol.EntityResult;
using Libol.Models;
using Libol.SupportClass;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Libol.Controllers
{
	public class AcquireReportController : Controller
	{
		LibolEntities le = new LibolEntities();
		AcquisitionBusiness ab = new AcquisitionBusiness();
		List<Temper> listTempt = new List<Temper>();
		FormatHoldingTitle format = new FormatHoldingTitle();

		public string GetContent(string copynumber)
		{
			string validate = copynumber.Replace("$a", " ");
			validate = validate.Replace("$b", " ");
			validate = validate.Replace("$c", " ");
			validate = validate.Replace("$n", " ");
			validate = validate.Replace("$p", " ");
			validate = validate.Replace("$e", " ");

			return validate.Trim();
		}
		[AuthAttribute(ModuleID = 4, RightID = "0")]
		public ActionResult AcquisitionIndex()
		{
			return View();
		}
		// GET: AcquireReport
		[AuthAttribute(ModuleID = 4, RightID = "27")]
		public ActionResult Index()
		{
			List<SelectListItem> lib = new List<SelectListItem>();
			lib.Add(new SelectListItem { Text = "Hãy chọn thư viện", Value = "" });
			foreach (var l in le.SP_HOLDING_LIB_SEL((int)Session["UserID"]).ToList())
			{
				lib.Add(new SelectListItem { Text = l.Code, Value = l.ID.ToString() });
			}
			ViewData["lib"] = lib;
			return View();
		}

		//GET LOCATIONS BY LIBRARY
		public JsonResult GetLocations(string id)
		{
			List<SelectListItem> loc = new List<SelectListItem>();
			loc.Add(new SelectListItem { Text = "Tất cả các kho", Value = "0" });
			if (!String.IsNullOrEmpty(id))
			{
				foreach (var l in le.SP_HOLDING_LIBLOCUSER_SEL((int)Session["UserID"], Int32.Parse(id)).ToList())
				{
					loc.Add(new SelectListItem { Text = l.Symbol, Value = l.ID.ToString() });
				}
			}
			return Json(new SelectList(loc, "Value", "Text"));
		}

		[HttpPost]
		public ActionResult BaoCaoBoSung_New(string Library, string LocationPrefix, string Location, string ReNumber, string StartDate, string EndDate, string SortBy, int? size, int? page, FormCollection collection)
		{

			String StartD = StartDate;
			String EndD = EndDate;
			int LibID = int.Parse(Library);
			int LocID = 0;
			List<int> locIDList = new List<int>();
			if (LocationPrefix != "0")
			{
				if (Location != "")
				{
					string[] s = Location.Split(',');
					for (int i = 0; i < s.Length; i++)
					{
						locIDList.Add(int.Parse(s[i]));
					}
				}
				else
				{
					foreach (var lbp in le.FPT_CIR_GET_LOCFULLNAME_LIBUSER_SEL((int)Session["UserID"], LibID, LocationPrefix))
					{
						locIDList.Add(lbp.ID);
					}

				}
			}
			else
			{
				locIDList.Add(LocID);
			}
			String sdd = "", edd = "";
			string po = "";
			sdd = StartDate;
			edd = EndDate;
			String orderby = "";
			orderby = SortBy;
			po = ReNumber;
			if (String.IsNullOrEmpty(po))
			{
				List<Temper> tpt = new List<Temper>();
				foreach (var locationID in locIDList)
				{
					LocID = locationID;
					if (orderby == "asc")
					{
						if (sdd != "" && edd != "")
						{
							DateTime sdt = Convert.ToDateTime(sdd);
							DateTime edt = Convert.ToDateTime(edd);
							//List<Temper> tpt = new List<Temper>();
							//đếm số nhập bằng cachs điếm sô lần xuất hiện của ISBN và kèm điều kiện ngày bổ xung hoặc locid
							foreach (var item in le.FPT_SP_GET_HOLDING_BY_LOCATIONID_lan12(LibID, LocID, null, sdt, edt, orderby).ToList())
							{
								int temp = Int32.Parse(item.ItemID.ToString());
								//check old or new book
								string check = "";
								foreach (var items in le.FPT_CHECK_ITEMID_AND_ACQUIREDATE(LibID,LocID, item.NgayBoSung.Value, temp).ToList())
								{

									check = items.ToString();
								}
								if (check != "")
								{
									String tpDKCB = item.DKCB;
									foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
									{
										// tpDKCB.Add(ites.DKCB, ites.ItemID);
										tpDKCB = ites.DKCB;
									}
									//string p = "", p2 = "";
									//p = item.NgayChungTu.ToString();
									//string thang = p.Substring(0, 2);
									//string ngay = p.Substring(3, 2);
									//string nam = p.Substring(6, 4);
									//p2 = ngay + "/" + thang + "/" + nam;
									tpt.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "cũ", item.ItemID, 0, 0));
								}
								else
								{
									String tpDKCB = item.DKCB;
									foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
									{
										// tpDKCB.Add(ites.DKCB, ites.ItemID);
										tpDKCB = ites.DKCB;
									}
									tpt.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "Mới", item.ItemID, 0, 0));

								}
								//List<int> listIn = ;
							}
							ViewBag.AcqItems = tpt;
							// ViewBag.tpt = tpt;
						}
						else if (edd != "" && sdd == "")
						{
							DateTime edt = Convert.ToDateTime(edd);
							foreach (var item in le.FPT_SP_GET_HOLDING_BY_LOCATIONID_lan12(LibID, LocID, null, null, edt, orderby).ToList())
							{
								int temp = Int32.Parse(item.ItemID.ToString());
								string check = "";
								foreach (var items in le.FPT_CHECK_ITEMID_AND_ACQUIREDATE(LibID, LocID, item.NgayBoSung.Value, temp).ToList())
								{

									check = items.ToString();
								}
								if (check != "")
								{
									//Temper tmpt = new Temper();
									string p = item.NgayChungTu.ToString();
									string p2 = item.NgayBoSung.ToString();
									string pp = item.DonGia.ToString();
									String tpDKCB = item.DKCB;
									foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
									{
										tpDKCB = ites.DKCB;
									}
									tpt.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "cũ", item.ItemID, 0, 0));
								}
								else
								{
									String tpDKCB = item.DKCB;
									foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
									{
										tpDKCB = ites.DKCB;
									}
									tpt.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "Mới", item.ItemID, 0, 0));

								}
								//List<int> listIn = ;
							}
							ViewBag.AcqItems = tpt;
						}
						else if (edd == "" && sdd != "")
						{
							DateTime sdt = Convert.ToDateTime(sdd);

							foreach (var item in le.FPT_SP_GET_HOLDING_BY_LOCATIONID_lan12(LibID, LocID, null, sdt, null, orderby).ToList())
							{
								int temp = Int32.Parse(item.ItemID.ToString());
								string check = "";
								foreach (var items in le.FPT_CHECK_ITEMID_AND_ACQUIREDATE(LibID, LocID, item.NgayBoSung.Value, temp).ToList())
								{

									check = items.ToString();
								}
								if (check != "")
								{
									string p = item.NgayChungTu.ToString();
									string p2 = item.NgayBoSung.ToString();
									string pp = item.DonGia.ToString();
									String tpDKCB = item.DKCB;
									foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
									{
										tpDKCB = ites.DKCB;
									}
									tpt.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "cũ", item.ItemID, 0, 0));
								}
								else
								{
									String tpDKCB = item.DKCB;
									foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
									{
										tpDKCB = ites.DKCB;
									}
									tpt.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "Mới", item.ItemID, 0, 0));

								}
							}
							ViewBag.AcqItems = tpt;
						}
						else if (sdd == "" && edd == "")
						{

							foreach (var item in le.FPT_SP_GET_HOLDING_BY_LOCATIONID_lan12(LibID, LocID, null, null, null, orderby).ToList())
							{
								int temp = Int32.Parse(item.ItemID.ToString());
								string check = "";
								foreach (var items in le.FPT_CHECK_ITEMID_AND_ACQUIREDATE(LibID, LocID, item.NgayBoSung.Value, temp).ToList())
								{

									check = items.ToString();
								}
								if (check != "")
								{
									string p = item.NgayChungTu.ToString();
									string p2 = item.NgayBoSung.ToString();
									string pp = item.DonGia.ToString();
									String tpDKCB = item.DKCB;
									foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
									{
										tpDKCB = ites.DKCB;
									}
									tpt.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "cũ", item.ItemID, 0, 0));
								}
								else
								{
									String tpDKCB = item.DKCB;
									foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
									{
										tpDKCB = ites.DKCB;
									}
									tpt.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "Mới", item.ItemID, 0, 0));

								}
								//List<int> listIn = ;
							}

							ViewBag.AcqItems = tpt;
						}
					}//other
					else if (orderby == "desc")
					{
						if (sdd != "" && edd != "")
						{
							DateTime sdt = Convert.ToDateTime(sdd);
							DateTime edt = Convert.ToDateTime(edd);
							//List<Temper> tpt = new List<Temper>();
							foreach (var item in le.FPT_SP_GET_HOLDING_BY_LOCATIONID_lan12(LibID, LocID, null, sdt, edt, orderby).ToList())
							{
								int temp = Int32.Parse(item.ItemID.ToString());
								string check = "";
								foreach (var items in le.FPT_CHECK_ITEMID_AND_ACQUIREDATE(LibID, LocID, item.NgayBoSung.Value, temp).ToList())
								{

									check = items.ToString();
								}
								if (check != "")
								{
									//Temper tmpt = new Temper();
									string p = item.NgayChungTu.ToString();
									string p2 = item.NgayBoSung.ToString();
									string pp = item.DonGia.ToString();
									String tpDKCB = item.DKCB;
									foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
									{
										// tpDKCB.Add(ites.DKCB, ites.ItemID);
										tpDKCB = ites.DKCB;
									}
									tpt.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "cũ", item.ItemID, 0, 0));
								}
								else
								{
									String tpDKCB = item.DKCB;
									foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
									{
										// tpDKCB.Add(ites.DKCB, ites.ItemID);
										tpDKCB = ites.DKCB;
									}
									tpt.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "Mới", item.ItemID, 0, 0));

								}
								//List<int> listIn = ;
							}
							ViewBag.AcqItems = tpt;
						}
						else if (edd != "" && sdd == "")
						{
							DateTime edt = Convert.ToDateTime(edd);
							//ViewBag.AcqItems = le.FPT_SP_GET_HOLDING_BYLOC_TIME(LocID, null, null, edt, orderby).ToList();
							// List<Temper> tpt = new List<Temper>();
							foreach (var item in le.FPT_SP_GET_HOLDING_BY_LOCATIONID_lan12(LibID, LocID, null, null, edt, orderby).ToList())
							{
								int temp = Int32.Parse(item.ItemID.ToString());
								string check = "";
								foreach (var items in le.FPT_CHECK_ITEMID_AND_ACQUIREDATE(LibID, LocID, item.NgayBoSung.Value, temp).ToList())
								{

									check = items.ToString();
								}
								if (check != "")
								{
									//Temper tmpt = new Temper();
									string p = item.NgayChungTu.ToString();
									string p2 = item.NgayBoSung.ToString();
									string pp = item.DonGia.ToString();
									String tpDKCB = item.DKCB;
									foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
									{
										// tpDKCB.Add(ites.DKCB, ites.ItemID);
										tpDKCB = ites.DKCB;
									}
									tpt.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "cũ", item.ItemID, 0, 0));
								}
								else
								{
									String tpDKCB = item.DKCB;
									foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
									{
										// tpDKCB.Add(ites.DKCB, ites.ItemID);
										tpDKCB = ites.DKCB;
									}
									tpt.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "Mới", item.ItemID, 0, 0));

								}
								//List<int> listIn = ;
							}
							ViewBag.AcqItems = tpt;
						}
						else if (edd == "" && sdd != "")
						{
							DateTime sdt = Convert.ToDateTime(sdd);
							//ViewBag.AcqItems = le.FPT_SP_GET_HOLDING_BYLOC_TIME(LocID, null, sdt, null, orderby).ToList();
							//List<Temper> tpt = new List<Temper>();
							foreach (var item in le.FPT_SP_GET_HOLDING_BY_LOCATIONID_lan12(LibID, LocID, null, sdt, null, orderby).ToList())
							{
								int temp = Int32.Parse(item.ItemID.ToString());
								string check = "";
								foreach (var items in le.FPT_CHECK_ITEMID_AND_ACQUIREDATE(LibID, LocID, item.NgayBoSung.Value, temp).ToList())
								{

									check = items.ToString();
								}
								if (check != "")
								{
									//Temper tmpt = new Temper();
									string p = item.NgayChungTu.ToString();
									string p2 = item.NgayBoSung.ToString();
									string pp = item.DonGia.ToString();

									String tpDKCB = item.DKCB;
									foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
									{
										// tpDKCB.Add(ites.DKCB, ites.ItemID);
										tpDKCB = ites.DKCB;
									}
									tpt.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "cũ", item.ItemID, 0, 0));
								}
								else
								{
									String tpDKCB = item.DKCB;
									foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
									{
										// tpDKCB.Add(ites.DKCB, ites.ItemID);
										tpDKCB = ites.DKCB;
									}
									tpt.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "Mới", item.ItemID, 0, 0));

								}
								//List<int> listIn = ;
							}
							ViewBag.AcqItems = tpt;
						}
						else if (sdd == "" && edd == "")
						{

							//ViewBag.AcqItems = le.FPT_SP_GET_HOLDING_BYLOC_TIME(LocID, null, null, null, orderby).ToList();
							//List<Temper> tpt = new List<Temper>();
							foreach (var item in le.FPT_SP_GET_HOLDING_BY_LOCATIONID_lan12(LibID, LocID, null, null, null, orderby).ToList())
							{
								int temp = Int32.Parse(item.ItemID.ToString());
								string check = "";
								foreach (var items in le.FPT_CHECK_ITEMID_AND_ACQUIREDATE(LibID, LocID, item.NgayBoSung.Value, temp).ToList())
								{

									check = items.ToString();
								}
								if (check != "")
								{
									//Temper tmpt = new Temper();
									string p = item.NgayChungTu.ToString();
									string p2 = item.NgayBoSung.ToString();
									string pp = item.DonGia.ToString();
									String tpDKCB = item.DKCB;
									foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
									{
										// tpDKCB.Add(ites.DKCB, ites.ItemID);
										tpDKCB = ites.DKCB;
									}
									tpt.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "cũ", item.ItemID, 0, 0));
								}
								else
								{
									String tpDKCB = item.DKCB;
									foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
									{
										// tpDKCB.Add(ites.DKCB, ites.ItemID);
										tpDKCB = ites.DKCB;
									}
									tpt.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "Mới", item.ItemID, 0, 0));

								}
							}
							ViewBag.AcqItems = tpt;
						}
					}
				}

				int slDauphay = 0;
				int slnhap = 0;
				int u = 1;
				int dem = 1;
				int indexGan = 0;
				int demso = 0;
				string ganString = "";


				///tinh so luot sach nhap
				foreach (var item in ViewBag.AcqItems)
				{
					//taoj mangr rooif check 2 phan tu lien tiep
					//lấy số lương sách nhập
					string nbs = "";
					nbs = Convert.ToString(item.NgayBoSung);
					if (nbs != "")
					{
						nbs = item.NgayBoSung;
						nbs = nbs.Substring(0, nbs.IndexOf(" "));
					}

					int itid = item.ItemID;
					Single dogia = Convert.ToSingle(item.DonGia);

					foreach (var itm in le.FPT_BORROWNUMBER(itid, dogia, nbs))
					{
						int check = -1;
						check = itm.Value;
						if (check != -1)
						{
							slnhap = Convert.ToInt32(check);
						}
					}
					item.SLN = slnhap;

					decimal gia = (decimal)item.DonGia;
					decimal a = item.SLN * gia;
					item.ThanhTien = (double)a;



				}


				foreach (var item in ViewBag.AcqItems)
				{
					string nbs = "";

					nbs = Convert.ToString(item.NgayBoSung);
					if (nbs != "")
					{
						nbs = item.NgayBoSung;
						nbs = nbs.Substring(0, nbs.IndexOf(" "));
					}

					int itid = item.ItemID;
					Single dogia = Convert.ToSingle(item.DonGia);


					foreach (var itm in le.FPT_SP_GET_COPYNUMBER_STRING(LibID, nbs, dogia, itid))
					{
						string sts = "";
						sts = itm.DKCB.ToString();
						if (sts != "")
						{
							item.DKCB = itm.DKCB;
						}
					}
				}

				//gộp DKCB
				foreach (var item in ViewBag.AcqItems)
				{
					string DKCBs = "";
					DKCBs = item.DKCB;
					char key = ',';
					for (int i = 0; i < DKCBs.Length; i++)
					{
						if (DKCBs[i] == key)
						{
							slDauphay++;

						}

					}
					slnhap = item.SLN;
					String[] arrDK = new string[slDauphay + 1];
					String[] arrDKfull = new string[slDauphay + 1];
					string h = item.DKCB;
					String ht = "";
					String strghep = "";
					string lastStr = "";

					if (slnhap > 1)
					{
						int indexDau = DKCBs.IndexOf(',');
						if (indexDau > 0)
						{
							ht = DKCBs.Substring(0, indexDau);
							lastStr = DKCBs.Substring(0, indexDau);
						}
						int bienphu = 0;
						string[] arrDKCBs = new string[slDauphay + 1];
						for (int i = 0; i < slDauphay; i++)
						{
							int checkDau = DKCBs.IndexOf(',');
							if (checkDau > 0)
							{
								string strTempt = DKCBs.Substring(0, checkDau);
								DKCBs = DKCBs.Substring(checkDau + 1);
								strTempt = strTempt.Substring(strTempt.Length - 6, 6);
								arrDKCBs[i] = strTempt;
							}
							bienphu++;

						}
						arrDKCBs[bienphu] = DKCBs.Substring(DKCBs.Length - 6, 6);

						//PHAN CU
						int kp = 0;
						for (int m = 0; m < arrDKCBs.Length; m++)
						{
							int n = m + 1;
							int intM = 0;
							int intN = 0;
							if (n < arrDKCBs.Length)
							{
								string strM = arrDKCBs[m];
								intM = Int32.Parse(strM);
								string strN = arrDKCBs[n];
								intN = Int32.Parse(strN); ;
								kp = intM + 1;
							}

							if (intN == kp)
							{
								if (n < arrDKCBs.Length)
								{
									indexGan = n;
									ganString = arrDKCBs[n];
									ganString = ganString.Substring(4, 2);
									demso++;
								}
								else
								{

								}
							}
							else if (n == arrDKCBs.Length - 1)
							{
								//lastStr = lastStr.Substring(lastStr.Length - 6, 6);
								if (lastStr == ht)
								{
									ganString = arrDKCBs[m];
									ganString = ganString.Substring(4, 2);
									if (indexGan > 0)
									{
										int ck = arrDKCBs.Length;
										if (indexGan == ck)
										{
											strghep = strghep + "-" + ganString;

										}
										else if (indexGan < ck)
										{
											strghep = strghep + "-" + ganString + ",";

										}

									}
									else
									{
										int ck = arrDKCBs.Length;
										if (indexGan == ck)
										{
											strghep = strghep + "," + ganString;
										}
										else if (indexGan < ck)
										{
											strghep = strghep + ganString + ",";
										}
									}
								}
								else
								{
									ganString = arrDKCBs[m];
									ganString = ganString.Substring(4, 2);
									int sDoi = Int32.Parse(ganString);
									int hieu = 0;
									hieu = sDoi - demso;
									if (hieu < 0)
									{
										hieu = hieu - (2 * hieu);
									}
									if (indexGan > 0)
									{
										int ck = arrDKCBs.Length;
										if (indexGan == ck)
										{

											strghep = strghep + hieu + "-" + ganString;
										}
										else if (indexGan < ck)
										{
											strghep = strghep + hieu + "-" + ganString + ",";
										}

									}
									else
									{
										int ck = arrDKCBs.Length;
										if (indexGan == ck)
										{
											strghep = strghep + "," + ganString;
										}
										else if (indexGan < ck)
										{
											strghep = strghep + ganString + ",";
										}
									}
								}
								ht = ganString;
								indexGan = 0;
								demso = 0;
							}
							else
							{
								if (lastStr == ht)
								{
									ganString = arrDKCBs[m];
									ganString = ganString.Substring(4, 2);
									if (indexGan > 0)
									{
										int ck = arrDKCBs.Length;
										if (indexGan < ck)
										{
											strghep = strghep + "-" + ganString + ",";

										}

									}
									else
									{
										int ck = arrDKCBs.Length;
										if (indexGan == ck)
										{
											strghep = strghep + ",";
										}
										else if (indexGan < ck)
										{
											strghep = strghep + ",";
										}
									}
								}
								else
								{
									ganString = arrDKCBs[m];
									ganString = ganString.Substring(4, 2);
									int sDoi = Int32.Parse(ganString);
									int hieu = 0;
									hieu = sDoi - demso;
									if (hieu < 0)
									{
										hieu = hieu - (2 * hieu);
									}
									if (indexGan > 0)
									{
										int ck = arrDKCBs.Length;
										if (indexGan == ck)
										{

											strghep = strghep + hieu + "-" + ganString;
										}
										else if (indexGan < ck)
										{
											strghep = strghep + hieu + "-" + ganString + ",";
										}

									}
									else
									{
										int ck = arrDKCBs.Length;
										if (indexGan == ck)
										{
											strghep = strghep + "," + ganString;
										}
										else if (indexGan < ck)
										{
											strghep = strghep + ganString + ",";
										}
									}
								}
								ht = ganString;
								indexGan = 0;
								demso = 0;
							}
							//}
						}
						u = dem;

						//CUOI
						if (strghep.LastIndexOf(',') > 0)
						{
							strghep = strghep.Substring(0, strghep.LastIndexOf(','));
						}

						item.DKCB = lastStr + strghep;

					}
					else if (slnhap == 1)
					{
						int hjk = 0;
						hjk = DKCBs.IndexOf(',');
						if (hjk == -1)
						{
							item.DKCB = DKCBs;
						}
						else
						{
							item.DKCB = DKCBs.Substring(0, hjk);
						}
						u++;

					}

					slDauphay = 0;
				}

			}
			else
			{
				List<Temper> listPO = new List<Temper>();
				int poiid = Convert.ToInt32(po);
				foreach (var locationID in locIDList)
				{
					LocID = locationID;
					if (sdd == "" && edd == "")
					{
						foreach (var item in le.FPT_SP_GET_HOLDING_BY_LOCATIONID_lan12(LibID, LocID, poiid, null, null, orderby).ToList())
						{
							String tpDKCB = item.DKCB;
							foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
							{
								// tpDKCB.Add(ites.DKCB, ites.ItemID);
								tpDKCB = ites.DKCB;
							}
							int uCount = 0;
							foreach (var itemss in le.FPT_SELECT_USECOUNT2(LibID, item.ItemID, item.NgayBoSung))
							{
								uCount += itemss.Value;
							}
							listPO.Add(new Temper(uCount, item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "", item.ItemID, 0, 0));
						}
						ViewBag.POList = listPO;
					}
					else if (sdd != "" && edd == "")
					{
						DateTime sdt = Convert.ToDateTime(sdd);
						foreach (var item in le.FPT_SP_GET_HOLDING_BY_LOCATIONID_lan12(LibID, LocID, poiid, sdt, null, orderby).ToList())
						{
							String tpDKCB = item.DKCB;
							foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
							{
								// tpDKCB.Add(ites.DKCB, ites.ItemID);
								tpDKCB = ites.DKCB;
							}
							int uCount = 0;
							foreach (var itemss in le.FPT_SELECT_USECOUNT2(LibID, item.ItemID, item.NgayBoSung))
							{
								uCount += itemss.Value;
							}
							listPO.Add(new Temper(uCount, item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "", item.ItemID, 0, 0));
						}
						ViewBag.POList = listPO;
					}
					else if (sdd == "" && edd != "")
					{
						DateTime edt = Convert.ToDateTime(edd);
						foreach (var item in le.FPT_SP_GET_HOLDING_BY_LOCATIONID_lan12(LibID, LocID, poiid, null, edt, orderby).ToList())
						{
							String tpDKCB = item.DKCB;
							foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
							{
								// tpDKCB.Add(ites.DKCB, ites.ItemID);
								tpDKCB = ites.DKCB;
							}
							int uCount = 0;
							foreach (var itemss in le.FPT_SELECT_USECOUNT2(LibID, item.ItemID, item.NgayBoSung))
							{
								uCount += itemss.Value;
							}
							listPO.Add(new Temper(uCount, item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "", item.ItemID, 0, 0));
						}
						ViewBag.POList = listPO;
					}
					else if (sdd != "" && edd != "")
					{
						DateTime sdt = Convert.ToDateTime(sdd);
						DateTime edt = Convert.ToDateTime(edd);
						foreach (var item in le.FPT_SP_GET_HOLDING_BY_LOCATIONID_lan12(LibID, LocID, poiid, sdt, edt, orderby).ToList())
						{
							String tpDKCB = item.DKCB;
							foreach (var ites in le.FPT_SP_JOIN_COPYNUMBER_BY_ITEMID_AND_ACQUIREDDATE(item.ItemID, item.NgayBoSung.Value).ToList())
							{
								// tpDKCB.Add(ites.DKCB, ites.ItemID);
								tpDKCB = ites.DKCB;
							}
							int uCount = 0;
							foreach (var itemss in le.FPT_SELECT_USECOUNT2(LibID, item.ItemID, item.NgayBoSung))
							{
								uCount += itemss.Value;
							}
							listPO.Add(new Temper(uCount, item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), tpDKCB, item.NgayBoSung.ToString(), item.IdNhaXuatBan, item.NamXuatBan, item.DonGia.Value, item.DonViTienTe, "", item.ItemID, 0, 0));
						}
						ViewBag.POList = listPO;
					}
				}

				int slDauphay = 0;
				int slnhap = 0;
				int u = 1;
				int dem = 1;
				int indexGan = 0;
				int demso = 0;
				string ganString = "";

				///tinh so luot sach nhap
				foreach (var item in ViewBag.POList)
				{
					//taoj mangr rooif check 2 phan tu lien tiep
					//lấy số lương sách nhập
					string nbs = "";
					nbs = Convert.ToString(item.NgayBoSung);
					if (nbs != "")
					{
						nbs = item.NgayBoSung;
						nbs = nbs.Substring(0, nbs.IndexOf(" "));
					}

					int itid = item.ItemID;
					Single dogia = Convert.ToSingle(item.DonGia);

					foreach (var itm in le.FPT_BORROWNUMBER(itid, dogia, nbs))
					{
						int check = -1;
						check = itm.Value;
						if (check != -1)
						{
							slnhap = Convert.ToInt32(check);
						}
					}
					item.SLN = slnhap;

					decimal gia = (decimal)item.DonGia;
					decimal a = item.SLN * gia;
					item.ThanhTien = (double)a;



				}


				foreach (var item in ViewBag.POList)
				{
					string nbs = "";

					nbs = Convert.ToString(item.NgayBoSung);
					if (nbs != "")
					{
						nbs = item.NgayBoSung;
						nbs = nbs.Substring(0, nbs.IndexOf(" "));
					}

					int itid = item.ItemID;
					Single dogia = Convert.ToSingle(item.DonGia);


					foreach (var itm in le.FPT_SP_GET_COPYNUMBER_STRING(LibID, nbs, dogia, itid))
					{
						string sts = "";
						sts = itm.DKCB.ToString();
						if (sts != "")
						{
							item.DKCB = itm.DKCB;
						}
					}
				}

				//gộp DKCB
				foreach (var item in ViewBag.POList)
				{
					string DKCBs = "";
					DKCBs = item.DKCB;
					char key = ',';
					for (int i = 0; i < DKCBs.Length; i++)
					{
						if (DKCBs[i] == key)
						{
							slDauphay++;

						}

					}
					slnhap = item.SLN;
					String[] arrDK = new string[slDauphay + 1];
					String[] arrDKfull = new string[slDauphay + 1];
					string h = item.DKCB;
					String ht = "";
					String strghep = "";
					string lastStr = "";

					if (slnhap > 1)
					{
						int indexDau = DKCBs.IndexOf(',');
						if (indexDau > 0)
						{
							ht = DKCBs.Substring(0, indexDau);
							lastStr = DKCBs.Substring(0, indexDau);
						}
						int bienphu = 0;
						string[] arrDKCBs = new string[slDauphay + 1];
						for (int i = 0; i < slDauphay; i++)
						{
							int checkDau = DKCBs.IndexOf(',');
							if (checkDau > 0)
							{
								string strTempt = DKCBs.Substring(0, checkDau);
								DKCBs = DKCBs.Substring(checkDau + 1);
								strTempt = strTempt.Substring(strTempt.Length - 6, 6);
								arrDKCBs[i] = strTempt;
							}
							bienphu++;

						}
						arrDKCBs[bienphu] = DKCBs.Substring(DKCBs.Length - 6, 6);

						//PHAN CU
						int kp = 0;
						for (int m = 0; m < arrDKCBs.Length; m++)
						{
							int n = m + 1;
							int intM = 0;
							int intN = 0;
							if (n < arrDKCBs.Length)
							{
								string strM = arrDKCBs[m];
								intM = Int32.Parse(strM);
								string strN = arrDKCBs[n];
								intN = Int32.Parse(strN); ;
								kp = intM + 1;
							}

							if (intN == kp)
							{
								if (n < arrDKCBs.Length)
								{
									indexGan = n;
									ganString = arrDKCBs[n];
									ganString = ganString.Substring(4, 2);
									demso++;
								}
								else
								{

								}
							}
							else if (n == arrDKCBs.Length - 1)
							{
								//lastStr = lastStr.Substring(lastStr.Length - 6, 6);
								if (lastStr == ht)
								{
									ganString = arrDKCBs[m];
									ganString = ganString.Substring(4, 2);
									if (indexGan > 0)
									{
										int ck = arrDKCBs.Length;
										if (indexGan == ck)
										{
											strghep = strghep + "-" + ganString;

										}
										else if (indexGan < ck)
										{
											strghep = strghep + "-" + ganString + ",";

										}

									}
									else
									{
										int ck = arrDKCBs.Length;
										if (indexGan == ck)
										{
											strghep = strghep + "," + ganString;
										}
										else if (indexGan < ck)
										{
											strghep = strghep + ganString + ",";
										}
									}
								}
								else
								{
									ganString = arrDKCBs[m];
									ganString = ganString.Substring(4, 2);
									int sDoi = Int32.Parse(ganString);
									int hieu = 0;
									hieu = sDoi - demso;
									if (hieu < 0)
									{
										hieu = hieu - (2 * hieu);
									}
									if (indexGan > 0)
									{
										int ck = arrDKCBs.Length;
										if (indexGan == ck)
										{

											strghep = strghep + hieu + "-" + ganString;
										}
										else if (indexGan < ck)
										{
											strghep = strghep + hieu + "-" + ganString + ",";
										}

									}
									else
									{
										int ck = arrDKCBs.Length;
										if (indexGan == ck)
										{
											strghep = strghep + "," + ganString;
										}
										else if (indexGan < ck)
										{
											strghep = strghep + ganString + ",";
										}
									}
								}
								ht = ganString;
								indexGan = 0;
								demso = 0;
							}
							else
							{
								if (lastStr == ht)
								{
									ganString = arrDKCBs[m];
									ganString = ganString.Substring(4, 2);
									if (indexGan > 0)
									{
										int ck = arrDKCBs.Length;
										if (indexGan < ck)
										{
											strghep = strghep + "-" + ganString + ",";

										}

									}
									else
									{
										int ck = arrDKCBs.Length;
										if (indexGan == ck)
										{
											strghep = strghep + ",";
										}
										else if (indexGan < ck)
										{
											strghep = strghep + ",";
										}
									}
								}
								else
								{
									ganString = arrDKCBs[m];
									ganString = ganString.Substring(4, 2);
									int sDoi = Int32.Parse(ganString);
									int hieu = 0;
									hieu = sDoi - demso;
									if (hieu < 0)
									{
										hieu = hieu - (2 * hieu);
									}
									if (indexGan > 0)
									{
										int ck = arrDKCBs.Length;
										if (indexGan == ck)
										{

											strghep = strghep + hieu + "-" + ganString;
										}
										else if (indexGan < ck)
										{
											strghep = strghep + hieu + "-" + ganString + ",";
										}

									}
									else
									{
										int ck = arrDKCBs.Length;
										if (indexGan == ck)
										{
											strghep = strghep + "," + ganString;
										}
										else if (indexGan < ck)
										{
											strghep = strghep + ganString + ",";
										}
									}
								}
								ht = ganString;
								indexGan = 0;
								demso = 0;
							}
							//}
						}
						u = dem;

						//CUOI
						if (strghep.LastIndexOf(',') > 0)
						{
							strghep = strghep.Substring(0, strghep.LastIndexOf(','));
						}

						item.DKCB = lastStr + strghep;

					}
					else if (slnhap == 1)
					{
						int hjk = 0;
						hjk = DKCBs.IndexOf(',');
						if (hjk == -1)
						{
							item.DKCB = DKCBs;
						}
						else
						{
							item.DKCB = DKCBs.Substring(0, hjk);
						}
						u++;

					}

					slDauphay = 0;
				}

			}

			List<Temper> display1 = new List<Temper>();
			List<Temper> display2 = new List<Temper>();
			List<Temper> display3 = new List<Temper>();
			List<Temper> display4 = new List<Temper>();
			List<Temper> display5 = new List<Temper>();
			List<Temper> display6 = new List<Temper>();
			Temper temp1 = null;
			Temper temp2 = null;
			Temper temp3 = null;
			Temper temp4 = null;
			Temper temp5 = null;
			Temper temp6 = null;

			if (ViewBag.AcqItems != null)
			{

				foreach (var item in ViewBag.AcqItems)
				{
					string st = "";
					try
					{
						st = item.DonViTienTe;
						if (st != null)
						{
							st = st.Replace(" ", "");
						}

						if (st == "VND")
						{
							string ngayct = "";
							string ngaybosug = "";
							ngayct = item.NgayChungTu.ToString();
							ngayct = ngayct.Substring(0, ngayct.IndexOf(" "));
							ngaybosug = item.NgayBoSung.ToString();
							ngaybosug = ngaybosug.Substring(0, ngaybosug.IndexOf(" "));
							if (ngayct != "")
							{
								temp1 = new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, ngayct, item.DKCB, ngaybosug, item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);

							}
							else
							{
								temp1 = new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), item.DKCB, item.NgayBoSung, item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);

							}


							Temper check = checkDup(display1, temp1);
							if (check == null)
							{
								display1.Add(temp1);
							}
							else
							{
								
								var year = check.NamXuatBan.Split(' ');
								bool checkYear = false;
								foreach(string tmp in year)
								{
									if(tmp == temp1.NamXuatBan)
									{
										checkYear = true;
									}
								}
								if (checkYear == false)
								{
									temp1.NamXuatBan += " " + check.NamXuatBan;
								}
								else
								{
									temp1.NamXuatBan = check.NamXuatBan;
								}
								var publisher = check.NhaXuatBan.Split(';');
								bool checkPublisher = false;
								foreach (string tmp in publisher)
								{
									if (tmp == temp1.NhaXuatBan)
									{
										checkPublisher = true;
									}
								}
								if (checkPublisher == false)
								{
									temp1.NhaXuatBan += ";" + check.NhaXuatBan;
								}
								else
								{
									temp1.NhaXuatBan = check.NhaXuatBan;
								}
								display1.Remove(check);
								display1.Add(temp1);
							}
						}
						if (st == "YEN")
						{
							string ngayct = "";
							string ngaybosug = "";
							ngayct = item.NgayChungTu.ToString();
							ngaybosug = item.NgayBoSung.ToString();
							if (ngayct != null)
							{
								ngayct = ngayct.Substring(0, ngayct.IndexOf(" "));
								ngaybosug = ngaybosug.Substring(0, ngaybosug.IndexOf(" "));
								temp2 = new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, ngayct, item.DKCB, ngaybosug, item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);
							}
							else
							{
								temp2 = new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), item.DKCB, ngaybosug, item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);

							}
							
							Temper check = checkDup(display2, temp2);
							if (check == null)
							{
								display2.Add(temp2);
							}
							else
							{

								var year = check.NamXuatBan.Split(' ');
								bool checkYear = false;
								foreach (string tmp in year)
								{
									if (tmp == temp1.NamXuatBan)
									{
										checkYear = true;
									}
								}
								if (checkYear == false)
								{
									temp2.NamXuatBan += " " + check.NamXuatBan;
								}
								else
								{
									temp2.NamXuatBan = check.NamXuatBan;
								}
								var publisher = check.NhaXuatBan.Split(';');
								bool checkPublisher = false;
								foreach (string tmp in publisher)
								{
									if (tmp == temp2.NhaXuatBan)
									{
										checkPublisher = true;
									}
								}
								if (checkPublisher == false)
								{
									temp2.NhaXuatBan += ";" + check.NhaXuatBan;
								}
								else
								{
									temp2.NhaXuatBan = check.NhaXuatBan;
								}
								display2.Remove(check);
								display2.Add(temp2);
							}
							// display2.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), item.DKCB, item.NgayBoSung.ToString(), item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien));
						}
						if (st == "USD")
						{
							string ngayct = "";
							string ngaybosug = "";
							ngayct = item.NgayChungTu.ToString();
							ngaybosug = item.NgayBoSung.ToString();
							if (ngayct != null)
							{
								ngayct = ngayct.Substring(0, ngayct.IndexOf(" "));
								ngaybosug = ngaybosug.Substring(0, ngaybosug.IndexOf(" "));
								temp3 = new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, ngayct, item.DKCB, ngaybosug, item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);
							}
							else
							{
								temp3 = new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), item.DKCB, ngaybosug, item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);

							}
							
							Temper check = checkDup(display3, temp3);
							if (check == null)
							{
								display3.Add(temp3);
							}
							else
							{

								var year = check.NamXuatBan.Split(' ');
								bool checkYear = false;
								foreach (string tmp in year)
								{
									if (tmp == temp3.NamXuatBan)
									{
										checkYear = true;
									}
								}
								if (checkYear == false)
								{
									temp3.NamXuatBan += " " + check.NamXuatBan;
								}
								else
								{
									temp3.NamXuatBan = check.NamXuatBan;
								}
								var publisher = check.NhaXuatBan.Split(';');
								bool checkPublisher = false;
								foreach (string tmp in publisher)
								{
									if (tmp == temp3.NhaXuatBan)
									{
										checkPublisher = true;
									}
								}
								if (checkPublisher == false)
								{
									temp3.NhaXuatBan += ";" + check.NhaXuatBan;
								}
								else
								{
									temp3.NhaXuatBan = check.NhaXuatBan;
								}
								display3.Remove(check);
								display3.Add(temp3);
							}
							// display3.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), item.DKCB, item.NgayBoSung.ToString(), item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien));
						}
						if (st == "B?NGANH")
						{
							string ngayct = "";
							string ngaybosug = "";
							ngayct = item.NgayChungTu.ToString();
							ngaybosug = item.NgayBoSung.ToString();
							if (ngayct != null)
							{
								ngayct = ngayct.Substring(0, ngayct.IndexOf(" "));
								ngaybosug = ngaybosug.Substring(0, ngaybosug.IndexOf(" "));
								temp4 = new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, ngayct, item.DKCB, ngaybosug, item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);
							}
							else
							{
								temp4 = new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), item.DKCB, ngaybosug, item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);

							}
							
							Temper check = checkDup(display4, temp4);
							if (check == null)
							{
								display4.Add(temp4);
							}
							else
							{

								var year = check.NamXuatBan.Split(' ');
								bool checkYear = false;
								foreach (string tmp in year)
								{
									if (tmp == temp4.NamXuatBan)
									{
										checkYear = true;
									}
								}
								if (checkYear == false)
								{
									temp4.NamXuatBan += " " + check.NamXuatBan;
								}
								else
								{
									temp4.NamXuatBan = check.NamXuatBan;
								}
								var publisher = check.NhaXuatBan.Split(';');
								bool checkPublisher = false;
								foreach (string tmp in publisher)
								{
									if (tmp == temp4.NhaXuatBan)
									{
										checkPublisher = true;
									}
								}
								if (checkPublisher == false)
								{
									temp4.NhaXuatBan += ";" + check.NhaXuatBan;
								}
								else
								{
									temp4.NhaXuatBan = check.NhaXuatBan;
								}
								display4.Remove(check);
								display4.Add(temp4);
							}
							//  display4.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), item.DKCB, item.NgayBoSung.ToString(), item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien));
						}
						if (st == "CENT")
						{
							string ngayct = "";
							string ngaybosug = "";
							ngayct = item.NgayChungTu.ToString();
							ngaybosug = item.NgayBoSung.ToString();
							if (ngayct != null)
							{
								ngayct = ngayct.Substring(0, ngayct.IndexOf(" "));
								ngaybosug = ngaybosug.Substring(0, ngaybosug.IndexOf(" "));
								temp5 = new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, ngayct, item.DKCB, ngaybosug, item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);
							}
							else
							{
								temp5 = new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), item.DKCB, ngaybosug, item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);

							}
							
							Temper check = checkDup(display5, temp5);
							if (check == null)
							{
								display5.Add(temp5);
							}
							else
							{

								var year = check.NamXuatBan.Split(' ');
								bool checkYear = false;
								foreach (string tmp in year)
								{
									if (tmp == temp5.NamXuatBan)
									{
										checkYear = true;
									}
								}
								if (checkYear == false)
								{
									temp5.NamXuatBan += " " + check.NamXuatBan;
								}
								else
								{
									temp5.NamXuatBan = check.NamXuatBan;
								}
								var publisher = check.NhaXuatBan.Split(';');
								bool checkPublisher = false;
								foreach (string tmp in publisher)
								{
									if (tmp == temp5.NhaXuatBan)
									{
										checkPublisher = true;
									}
								}
								if (checkPublisher == false)
								{
									temp5.NhaXuatBan += ";" + check.NhaXuatBan;
								}
								else
								{
									temp5.NhaXuatBan = check.NhaXuatBan;
								}
								display5.Remove(check);
								display5.Add(temp5);
							}
							// display5.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), item.DKCB, item.NgayBoSung.ToString(), item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien));
						}
						if (st == "EUR")
						{
							string ngayct = "";
							string ngaybosug = "";
							ngayct = item.NgayChungTu.ToString();
							ngaybosug = item.NgayBoSung.ToString();
							if (ngayct != null)
							{
								ngayct = ngayct.Substring(0, ngayct.IndexOf(" "));
								ngaybosug = ngaybosug.Substring(0, ngaybosug.IndexOf(" "));
								temp6 = new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, ngayct, item.DKCB, ngaybosug, item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);
							}
							else
							{
								temp6 = new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), item.DKCB, ngaybosug, item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);

							}
							Temper check = checkDup(display6, temp6);
							if (check == null)
							{
								display6.Add(temp6);
							}
							else
							{

								var year = check.NamXuatBan.Split(' ');
								bool checkYear = false;
								foreach (string tmp in year)
								{
									if (tmp == temp6.NamXuatBan)
									{
										checkYear = true;
									}
								}
								if (checkYear == false)
								{
									temp6.NamXuatBan += " " + check.NamXuatBan;
								}
								else
								{
									temp6.NamXuatBan = check.NamXuatBan;
								}
								var publisher = check.NhaXuatBan.Split(';');
								bool checkPublisher = false;
								foreach (string tmp in publisher)
								{
									if (tmp == temp6.NhaXuatBan)
									{
										checkPublisher = true;
									}
								}
								if (checkPublisher == false)
								{
									temp6.NhaXuatBan += ";" + check.NhaXuatBan;
								}
								else
								{
									temp6.NhaXuatBan = check.NhaXuatBan;
								}
								display6.Remove(check);
								display6.Add(temp6);
							}
							
							//display6.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), item.DKCB, item.NgayBoSung.ToString(), item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien));
						}

					}
					catch (Exception e)
					{
						e.ToString();
					}

				}

			}
			else if (ViewBag.POList != null)
			{
				foreach (var item in ViewBag.POList)
				{
					string st = "";

					st = item.DonViTienTe;
					if (st != null)
					{
						st = st.Replace(" ", "");
					}

					if (st == "VND")
					{
						string ngayct = "";
						string ngaybosug = "";
						ngayct = item.NgayChungTu.ToString();
						ngaybosug = item.NgayBoSung.ToString();
						if (ngayct != null)
						{
							int ngaycct = ngayct.IndexOf(" ");
							if (ngaycct > 0)
							{
								ngayct = ngayct.Substring(0, ngaycct);
							}
							ngaybosug = ngaybosug.Substring(0, ngaybosug.IndexOf(" "));
							temp1 = new Temper(item.UseCount, item.POID, item.SoChungTu, item.NhanDe, item.ISBN, ngayct,
								item.DKCB, ngaybosug, item.NhaXuatBan, item.NamXuatBan, item.DonGia,
								item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);
						}
						else
						{
							temp1 = new Temper(item.UseCount, item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu,
								item.DKCB, item.NgayBoSung, item.NhaXuatBan, item.NamXuatBan, item.DonGia,
								item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);
						}

						display1.Add(temp1);
					}
					if (st == "YEN")
					{
						string ngayct = "";
						string ngaybosug = "";
						ngayct = item.NgayChungTu.ToString();
						ngaybosug = item.NgayBoSung.ToString();
						if (ngayct != null)
						{
							int ngaycct = ngayct.IndexOf(" ");
							if (ngaycct > 0)
							{
								ngayct = ngayct.Substring(0, ngaycct);
							}
							ngaybosug = ngaybosug.Substring(0, ngaybosug.IndexOf(" "));
							temp2 = new Temper(item.UseCount, item.POID, item.SoChungTu, item.NhanDe, item.ISBN, ngayct,
								item.DKCB, ngaybosug, item.NhaXuatBan, item.NamXuatBan, item.DonGia,
								item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);
						}
						else
						{
							temp2 = new Temper(item.UseCount, item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu,
								item.DKCB, item.NgayBoSung, item.NhaXuatBan, item.NamXuatBan, item.DonGia,
								item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);
						}
						display2.Add(temp2);
						// display2.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), item.DKCB, item.NgayBoSung.ToString(), item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien));
					}
					if (st == "USD")
					{
						string ngayct = "";
						string ngaybosug = "";
						ngayct = item.NgayChungTu.ToString();
						ngaybosug = item.NgayBoSung.ToString();
						if (ngayct != null)
						{
							int ngaycct = ngayct.IndexOf(" ");
							if (ngaycct > 0)
							{
								ngayct = ngayct.Substring(0, ngaycct);
							}

							ngaybosug = ngaybosug.Substring(0, ngaybosug.IndexOf(" "));
							temp3 = new Temper(item.UseCount, item.POID, item.SoChungTu, item.NhanDe, item.ISBN, ngayct,
								item.DKCB, ngaybosug, item.NhaXuatBan, item.NamXuatBan, item.DonGia,
								item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);
						}
						else
						{
							temp3 = new Temper(item.UseCount, item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu,
								item.DKCB, item.NgayBoSung, item.NhaXuatBan, item.NamXuatBan, item.DonGia,
								item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);
						}
						display3.Add(temp3);
						// display3.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), item.DKCB, item.NgayBoSung.ToString(), item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien));
					}
					if (st == "B?NGANH")
					{
						string ngayct = "";
						string ngaybosug = "";
						ngayct = item.NgayChungTu.ToString();
						ngaybosug = item.NgayBoSung.ToString();
						if (ngayct != null)
						{
							int ngaycct = ngayct.IndexOf(" ");
							if (ngaycct > 0)
							{
								ngayct = ngayct.Substring(0, ngaycct);
							}
							ngaybosug = ngaybosug.Substring(0, ngaybosug.IndexOf(" "));
							temp4 = new Temper(item.UseCount, item.POID, item.SoChungTu, item.NhanDe, item.ISBN, ngayct,
								item.DKCB, ngaybosug, item.NhaXuatBan, item.NamXuatBan, item.DonGia,
								item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);
						}
						else
						{
							temp4 = new Temper(item.UseCount, item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu,
								item.DKCB, item.NgayBoSung, item.NhaXuatBan, item.NamXuatBan, item.DonGia,
								item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);
						}
						display4.Add(temp4);
						//  display4.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), item.DKCB, item.NgayBoSung.ToString(), item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien));
					}
					if (st == "CENT")
					{
						string ngayct = "";
						string ngaybosug = "";
						ngayct = item.NgayChungTu.ToString();
						ngaybosug = item.NgayBoSung.ToString();
						if (ngayct != null)
						{
							int ngaycct = ngayct.IndexOf(" ");
							if (ngaycct > 0)
							{
								ngayct = ngayct.Substring(0, ngaycct);
							}
							ngaybosug = ngaybosug.Substring(0, ngaybosug.IndexOf(" "));
							temp5 = new Temper(item.UseCount, item.POID, item.SoChungTu, item.NhanDe, item.ISBN, ngayct,
								item.DKCB, ngaybosug, item.NhaXuatBan, item.NamXuatBan, item.DonGia,
								item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);
						}
						else
						{
							temp5 = new Temper(item.UseCount, item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu,
								item.DKCB, item.NgayBoSung, item.NhaXuatBan, item.NamXuatBan, item.DonGia,
								item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);
						}
						display5.Add(temp5);
						// display5.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), item.DKCB, item.NgayBoSung.ToString(), item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien));
					}
					if (st == "EUR")
					{
						string ngayct = "";
						string ngaybosug = "";
						ngayct = item.NgayChungTu.ToString();
						ngaybosug = item.NgayBoSung.ToString();
						if (ngayct != null)
						{
							int ngaycct = ngayct.IndexOf(" ");
							if (ngaycct > 0)
							{
								ngayct = ngayct.Substring(0, ngaycct);
							}
							ngaybosug = ngaybosug.Substring(0, ngaybosug.IndexOf(" "));
							temp6 = new Temper(item.UseCount, item.POID, item.SoChungTu, item.NhanDe, item.ISBN, ngayct,
								item.DKCB, ngaybosug, item.NhaXuatBan, item.NamXuatBan, item.DonGia,
								item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);
						}
						else
						{
							temp6 = new Temper(item.UseCount, item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu,
								item.DKCB, item.NgayBoSung, item.NhaXuatBan, item.NamXuatBan, item.DonGia,
								item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien);
						}
						display6.Add(temp6);
						//display6.Add(new Temper(item.POID, item.SoChungTu, item.NhanDe, item.ISBN, item.NgayChungTu.ToString(), item.DKCB, item.NgayBoSung.ToString(), item.NhaXuatBan, item.NamXuatBan, item.DonGia, item.DonViTienTe, item.TinhTrangSach, item.ItemID, item.SLN, item.ThanhTien));
					}
				}
			}
			//check null VND
			if (display1.Count == 0)
			{
				ViewBag.DisVND = null;
			}
			else
			{
				ViewBag.DisVND = display1.ToList();

			}
			//check null
			if (display2.Count == 0)
			{
				ViewBag.DisYEN = null;
			}
			else
			{
				ViewBag.DisYEN = display2;
			}
			//check null
			if (display3.Count == 0)
			{
				ViewBag.DisUSD = null;
			}
			else
			{
				ViewBag.DisUSD = display3.ToList();
			}
			//check null
			if (display4.Count == 0)
			{
				ViewBag.DisBAnh = null;
			}
			else
			{
				ViewBag.DisBAnh = display4.ToList();
			}

			//check null
			if (display5.Count == 0)
			{
				ViewBag.DisCENT = null;
			}
			else
			{
				ViewBag.DisCENT = display5.ToList();
			}
			//check null
			if (display6.Count == 0)
			{
				ViewBag.DisEUR = null;
			}
			else
			{
				ViewBag.DisEUR = display6.ToList();
			}


			return View();
		}

		[AuthAttribute(ModuleID = 4, RightID = "127")]
		public ActionResult AcquireStatisticIndex()
		{
			return View();
		}
		[AuthAttribute(ModuleID = 4, RightID = "28")]
		public ActionResult LanguageStat()
		{
			List<SelectListItem> lib = new List<SelectListItem>
			{
				new SelectListItem { Text = "Hãy chọn thư viện", Value = "" }
			};
			foreach (var l in le.SP_HOLDING_LIB_SEL((int)Session["UserID"]).ToList())
			{
				lib.Add(new SelectListItem { Text = l.Code, Value = l.ID.ToString() });
			}
			ViewData["lib"] = lib;
			return View();
		}
		[HttpPost]
		public PartialViewResult GetLanguageStats(string strLibID)
		{
			int LibID = 0;
			if (!String.IsNullOrEmpty(strLibID)) LibID = Convert.ToInt32(strLibID);
			ViewBag.Result = le.FPT_ACQ_LANGUAGE_STATISTIC(LibID).First();
			ViewBag.ItemDetailsResult = le.FPT_ACQ_LANGUAGE_DETAILS_STATISTIC("ITEM", LibID).ToList();
			ViewBag.CopyDetailsResult = le.FPT_ACQ_LANGUAGE_DETAILS_STATISTIC("COPY", LibID).ToList();
			return PartialView("GetLanguageStats");
		}
		[AuthAttribute(ModuleID = 4, RightID = "28")]
		public ActionResult StatisticYear()
		{
			List<SelectListItem> lib = new List<SelectListItem>
			{
				new SelectListItem { Text = "Hãy chọn thư viện", Value = "" }
			};
			foreach (var l in le.SP_HOLDING_LIB_SEL((int)Session["UserID"]).ToList())
			{
				lib.Add(new SelectListItem { Text = l.Code, Value = l.ID.ToString() });
			}
			ViewData["lib"] = lib;
			return View();
		}
		[HttpPost]
		public PartialViewResult GetYearStats(string strLibID, string strLocPrefix, string strLocID, string strFromYear, string strToYear)
		{
			int LibID = 0;
			//int LocID = 0;
			if (!String.IsNullOrEmpty(strLibID)) LibID = Convert.ToInt32(strLibID);
			//if (!String.IsNullOrEmpty(strLocID)) LocID = Convert.ToInt32(strLocID);
			ViewBag.Result = ab.FPT_ACQ_YEAR_STATISTIC_LIST(LibID, strLocPrefix, strLocID, strFromYear, strToYear, (int)Session["UserID"]);
			return PartialView("GetYearStats");
		}
		[AuthAttribute(ModuleID = 4, RightID = "28")]
		public ActionResult StatisticMonth()
		{
			List<SelectListItem> lib = new List<SelectListItem>
			{
				new SelectListItem { Text = "Hãy chọn thư viện", Value = "" }
			};
			foreach (var l in le.SP_HOLDING_LIB_SEL((int)Session["UserID"]).ToList())
			{
				lib.Add(new SelectListItem { Text = l.Code, Value = l.ID.ToString() });
			}
			ViewData["lib"] = lib;
			return View();
		}
		[HttpPost]
		public PartialViewResult GetMonthStats(string strLibID, string strLocPrefix, string strLocID, string strDateFrom, string strDateTo)
		{
			int LibID = 0;
			//int LocID = 0;
			if (!String.IsNullOrEmpty(strLibID)) LibID = Convert.ToInt32(strLibID);
			//if (!String.IsNullOrEmpty(strLocID)) LocID = Convert.ToInt32(strLocID);
			ViewBag.Result = ab.FPT_ACQ_MONTH_STATISTIC_LIST(LibID, strLocPrefix, strLocID, strDateFrom, strDateTo, (int)Session["UserID"]);
			return PartialView("GetMonthStats");
		}
		[AuthAttribute(ModuleID = 4, RightID = "27")]
		public ActionResult LiquidationStats()
		{
			List<SelectListItem> lib = new List<SelectListItem>
			{
				new SelectListItem { Text = "Hãy chọn thư viện", Value = "" }
			};
			foreach (var l in le.SP_HOLDING_LIB_SEL((int)Session["UserID"]).ToList())
			{
				lib.Add(new SelectListItem { Text = l.Code, Value = l.ID.ToString() });
			}
			ViewData["lib"] = lib;
			return View();
		}
		public PartialViewResult GetLiquidationStats(string strLiquidID, string strLibID, string strLocPrefix, string strLocID, string strFromDate, string strToDate)
		{
			ViewBag.LiquidCode = strLiquidID;
			return PartialView("GetLiquidationStats");
		}

		[HttpPost]
		public JsonResult GetLiquidationInfo(DataTableAjaxPostModel model, string strLiquidID, string strLibID, string strLocPrefix, string strLocID, string strFromDate, string strToDate)
		{
			int LibID = 0;
			//int LocID = 0;
			if (!String.IsNullOrEmpty(strLibID)) LibID = Convert.ToInt32(strLibID);
			//if (!String.IsNullOrEmpty(strLocID)) LocID = Convert.ToInt32(strLocID);
			var copy = ab.FPT_GET_LIQUIDBOOKS_LIST(strLiquidID, LibID, strLocPrefix, strLocID, strFromDate, strToDate, (int)Session["UserID"]);
			var search = copy.Where(a => true);
			decimal total = 0;
			if (model.search.value != null)
			{
				string searchValue = model.search.value;
				search = search.Where(a => a.LibName.ToUpper().Contains(searchValue.ToUpper())
					|| a.LocName.ToUpper().Contains(searchValue.ToUpper())
					|| a.CopyNumber.ToUpper().Contains(searchValue.ToUpper())
					|| a.Content.ToUpper().Contains(searchValue.ToUpper())
					|| a.Price.ToString().ToUpper().Contains(searchValue.ToUpper())
					|| a.RemovedDate.Value.ToString("dd/MM/yyyy").Contains(searchValue)
				);
			}
			var sorting = search.OrderBy(a => false);
			if (model.order[0].column == 0)
			{
				if (model.order[0].dir.Equals("asc"))
				{
					sorting = search.OrderBy(a => a.LibName);
				}
				else
				{
					sorting = search.OrderByDescending(a => a.LibName);
				}
			}
			else if (model.order[0].column == 1)
			{
				if (model.order[0].dir.Equals("asc"))
				{
					sorting = search.OrderBy(a => a.LocName);
				}
				else
				{
					sorting = search.OrderByDescending(a => a.LocName);
				}
			}
			else if (model.order[0].column == 2)
			{
				if (model.order[0].dir.Equals("asc"))
				{
					sorting = search.OrderBy(a => a.CopyNumber);
				}
				else
				{
					sorting = search.OrderByDescending(a => a.CopyNumber);
				}
			}
			else if (model.order[0].column == 3)
			{
				if (model.order[0].dir.Equals("asc"))
				{
					sorting = search.OrderBy(a => a.Content);
				}
				else
				{
					sorting = search.OrderByDescending(a => a.Content);
				}
			}
			else if (model.order[0].column == 4)
			{
				if (model.order[0].dir.Equals("asc"))
				{
					sorting = search.OrderBy(a => a.RemovedDate);
				}
				else
				{
					sorting = search.OrderByDescending(a => a.RemovedDate);
				}
			}
			else if (model.order[0].column == 5)
			{
				if (model.order[0].dir.Equals("asc"))
				{
					sorting = search.OrderBy(a => a.Price);
				}
				else
				{
					sorting = search.OrderByDescending(a => a.Price);
				}
			}
			var paging = sorting.Skip(model.start).Take(model.length).ToList();
			List<FPT_GET_LIQUIDBOOKS_Result_2> result = new List<FPT_GET_LIQUIDBOOKS_Result_2>();
			foreach (var i in paging)
			{
				result.Add(new FPT_GET_LIQUIDBOOKS_Result_2()
				{
					LibName = i.LibName,
					LocName = i.LocName,
					CopyNumber = i.CopyNumber,
					Content = format.OnFormatHoldingTitle(i.Content),
					RemovedDate = i.RemovedDate.Value.ToString("dd/MM/yyyy"),
					Price = i.Price.ToString("#.##")
				});
			}
			foreach (var i in search)
			{
				total += i.Price;
			}
			return Json(new
			{
				draw = model.draw,
				recordsTotal = copy.Count(),
				recordsFiltered = search.Count(),
				total,
				data = result
			});
		}
		[AuthAttribute(ModuleID = 4, RightID = "27")]
		public ActionResult RecomendReport()
		{
			List<SelectListItem> lib = new List<SelectListItem>();
			lib.Add(new SelectListItem { Text = "Hãy chọn thư viện", Value = "" });
			foreach (var l in le.SP_HOLDING_LIB_SEL((int)Session["UserID"]).ToList())
			{
				lib.Add(new SelectListItem { Text = l.Code, Value = l.ID.ToString() });
			}
			ViewData["lib"] = lib;
			return View();
		}
		public ActionResult GetRecomendReport(string Library, string LocationPrefix, string Location, string ReNumber, string StartDate, string EndDate, string RecordNumber, int? SortBy, int? size, int? page, FormCollection collection)
		{
			//String StartD = StartDate.ToString();
			//String EndD = EndDate.ToString();
			int LibID = int.Parse(Library);
			int LocID = 0;
			List<int> locIDList = new List<int>();
			if (LocationPrefix != "0")
			{
				if (Location != "")
				{
					string[] s = Location.Split(',');
					for (int i = 0; i < s.Length; i++)
					{
						locIDList.Add(int.Parse(s[i]));
					}
				}
				else
				{
					foreach (var lbp in le.FPT_CIR_GET_LOCFULLNAME_LIBUSER_SEL((int)Session["UserID"], LibID, LocationPrefix))
					{
						locIDList.Add(lbp.ID);
					}

				}
			}
			else
			{
				locIDList.Add(LocID);
			}
			String sdd = "", edd = "";
			string recomCode = "";
			sdd = StartDate;
			edd = EndDate;
			if (String.IsNullOrEmpty(ReNumber))
			{
				recomCode = null;
			}
			else
			{
				recomCode = ReNumber;
				recomCode = recomCode.Replace(" ", "");
			}

			if (sdd == "")
			{
				sdd = null;
			}

			if (edd == "")
			{
				edd = null;
			}

			List<Temper> listPO = new List<Temper>();

			List<FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result> listRecommend = new List<FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result>();
			foreach (var locationID in locIDList)
			{
				LocID = locationID;
				foreach (var item in ab.FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest(LibID, LocID, recomCode, sdd, edd, RecordNumber).ToList())
				{

					int uCount = 0;
					foreach (var itemss in le.FPT_SELECT_USECOUNT2(LibID, item.ItemID, item.ACQUIREDDATE))
					{
						uCount += itemss.Value;
					}
					string isb = "";
					foreach (var ite in le.FPT_JOIN_ISBN(item.ItemID))
					{
						isb = ite.ISBN;
					}
					FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result obj = new FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result()
					{
						RECORDNUMBER = item.RECORDNUMBER,
						Title = item.Title,
						ReceiptedDate = item.ReceiptedDate,
						useCount = uCount,
						ISBN = isb,
						InBookNum = 0,
						DKCB = "",
						ACQUIREDDATE = item.ACQUIREDDATE,
						LocationID = item.LocationID,
						RECOMMENDID = item.RECOMMENDID,
						Year = item.Year,
						Price = item.Price,
						Currency = item.Currency,
						NXB = item.NXB,
						FullPrice = 0,
						ItemID = item.ItemID,
						DateLastUsed = item.DateLastUsed
					};
					listRecommend.Add(obj);
				}
			}
			ViewBag.POList = listRecommend;





			int slDauphay = 0;
			int slnhap = 0;
			int u = 1;
			int dem = 1;
			int indexGan = 0;
			int demso = 0;
			string ganString = "";

			///tinh so luot sach nhap
			foreach (var item in ViewBag.POList)
			{
				//taoj mangr rooif check 2 phan tu lien tiep
				//lấy số lương sách nhập
				string nbs = "";
				nbs = Convert.ToString(item.ACQUIREDDATE);
				if (nbs != "")
				{
					// nbs = item.ACQUIREDDATE;
					nbs = nbs.Substring(0, nbs.IndexOf(" "));
				}

				int itid = item.ItemID;
				Single dogia = Convert.ToSingle(item.Price);

				foreach (var itm in le.FPT_BORROWNUMBER(itid, dogia, nbs))
				{
					int check = -1;
					check = itm.Value;
					if (check != -1)
					{
						slnhap = Convert.ToInt32(check);
					}
				}
				item.InBookNum = slnhap;

				decimal gia = (decimal)item.Price;
				item.FullPrice = item.InBookNum * item.Price;
				//item.FullPrice = (double)a;



			}


			foreach (var item in ViewBag.POList)
			{
				string nbs = "";

				nbs = Convert.ToString(item.ACQUIREDDATE);
				if (nbs != "")
				{
					//nbs = item.ACQUIREDDATEg;
					nbs = nbs.Substring(0, nbs.IndexOf(" "));
				}

				int itid = item.ItemID;
				Single dogia = Convert.ToSingle(item.Price);


				foreach (var itm in le.FPT_SP_GET_COPYNUMBER_STRING(LibID, nbs, dogia, itid))
				{
					string sts = "";
					sts = itm.DKCB.ToString();
					if (sts != "")
					{
						item.DKCB = itm.DKCB;
					}
				}
			}

			//gộp DKCB
			foreach (var item in ViewBag.POList)
			{
				string DKCBs = "";
				DKCBs = item.DKCB;
				char key = ',';
				for (int i = 0; i < DKCBs.Length; i++)
				{
					if (DKCBs[i] == key)
					{
						slDauphay++;

					}

				}
				slnhap = item.InBookNum;
				String[] arrDK = new string[slDauphay + 1];
				String[] arrDKfull = new string[slDauphay + 1];
				string h = item.DKCB;
				String ht = "";
				String strghep = "";
				string lastStr = "";

				if (slnhap > 1)
				{
					int indexDau = DKCBs.IndexOf(',');
					if (indexDau > 0)
					{
						ht = DKCBs.Substring(0, indexDau);
						lastStr = DKCBs.Substring(0, indexDau);
					}
					int bienphu = 0;
					string[] arrDKCBs = new string[slDauphay + 1];
					for (int i = 0; i < slDauphay; i++)
					{
						int checkDau = DKCBs.IndexOf(',');
						if (checkDau > 0)
						{
							string strTempt = DKCBs.Substring(0, checkDau);
							DKCBs = DKCBs.Substring(checkDau + 1);
							strTempt = strTempt.Substring(strTempt.Length - 6, 6);
							arrDKCBs[i] = strTempt;
						}
						bienphu++;

					}
					arrDKCBs[bienphu] = DKCBs.Substring(DKCBs.Length - 6, 6);

					//PHAN CU
					int kp = 0;
					for (int m = 0; m < arrDKCBs.Length; m++)
					{
						int n = m + 1;
						int intM = 0;
						int intN = 0;
						if (n < arrDKCBs.Length)
						{
							string strM = arrDKCBs[m];
							intM = Int32.Parse(strM);
							string strN = arrDKCBs[n];
							intN = Int32.Parse(strN); ;
							kp = intM + 1;
						}

						if (intN == kp)
						{
							if (n < arrDKCBs.Length)
							{
								indexGan = n;
								ganString = arrDKCBs[n];
								ganString = ganString.Substring(4, 2);
								demso++;
							}
							else
							{

							}
						}
						else if (n == arrDKCBs.Length - 1)
						{
							//lastStr = lastStr.Substring(lastStr.Length - 6, 6);
							if (lastStr == ht)
							{
								ganString = arrDKCBs[m];
								ganString = ganString.Substring(4, 2);
								if (indexGan > 0)
								{
									int ck = arrDKCBs.Length;
									if (indexGan == ck)
									{
										strghep = strghep + "-" + ganString;

									}
									else if (indexGan < ck)
									{
										strghep = strghep + "-" + ganString + ",";

									}

								}
								else
								{
									int ck = arrDKCBs.Length;
									if (indexGan == ck)
									{
										strghep = strghep + "," + ganString;
									}
									else if (indexGan < ck)
									{
										strghep = strghep + ganString + ",";
									}
								}
							}
							else
							{
								ganString = arrDKCBs[m];
								ganString = ganString.Substring(4, 2);
								int sDoi = Int32.Parse(ganString);
								int hieu = 0;
								hieu = sDoi - demso;
								if (hieu < 0)
								{
									hieu = hieu - (2 * hieu);
								}
								if (indexGan > 0)
								{
									int ck = arrDKCBs.Length;
									if (indexGan == ck)
									{

										strghep = strghep + hieu + "-" + ganString;
									}
									else if (indexGan < ck)
									{
										strghep = strghep + hieu + "-" + ganString + ",";
									}

								}
								else
								{
									int ck = arrDKCBs.Length;
									if (indexGan == ck)
									{
										strghep = strghep + "," + ganString;
									}
									else if (indexGan < ck)
									{
										strghep = strghep + ganString + ",";
									}
								}
							}
							ht = ganString;
							indexGan = 0;
							demso = 0;
						}
						else
						{
							if (lastStr == ht)
							{
								ganString = arrDKCBs[m];
								ganString = ganString.Substring(4, 2);
								if (indexGan > 0)
								{
									int ck = arrDKCBs.Length;
									if (indexGan < ck)
									{
										strghep = strghep + "-" + ganString + ",";

									}

								}
								else
								{
									int ck = arrDKCBs.Length;
									if (indexGan == ck)
									{
										strghep = strghep + ",";
									}
									else if (indexGan < ck)
									{
										strghep = strghep + ",";
									}
								}
							}
							else
							{
								ganString = arrDKCBs[m];
								ganString = ganString.Substring(4, 2);
								int sDoi = Int32.Parse(ganString);
								int hieu = 0;
								hieu = sDoi - demso;
								if (hieu < 0)
								{
									hieu = hieu - (2 * hieu);
								}
								if (indexGan > 0)
								{
									int ck = arrDKCBs.Length;
									if (indexGan == ck)
									{

										strghep = strghep + hieu + "-" + ganString;
									}
									else if (indexGan < ck)
									{
										strghep = strghep + hieu + "-" + ganString + ",";
									}

								}
								else
								{
									int ck = arrDKCBs.Length;
									if (indexGan == ck)
									{
										strghep = strghep + "," + ganString;
									}
									else if (indexGan < ck)
									{
										strghep = strghep + ganString + ",";
									}
								}
							}
							ht = ganString;
							indexGan = 0;
							demso = 0;
						}
						//}
					}
					u = dem;

					//CUOI
					if (strghep.LastIndexOf(',') > 0)
					{
						strghep = strghep.Substring(0, strghep.LastIndexOf(','));
					}

					item.DKCB = lastStr + strghep;

				}
				else if (slnhap == 1)
				{
					int hjk = 0;
					hjk = DKCBs.IndexOf(',');
					if (hjk == -1)
					{
						item.DKCB = DKCBs;
					}
					else
					{
						item.DKCB = DKCBs.Substring(0, hjk);
					}
					u++;

				}

				slDauphay = 0;
			}

			// }

			List<FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result> display1 = new List<FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result>();
			List<FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result> display2 = new List<FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result>();
			List<FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result> display3 = new List<FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result>();
			List<FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result> display4 = new List<FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result>();
			List<FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result> display5 = new List<FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result>();
			List<FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result> display6 = new List<FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result>();

			if (ViewBag.POList != null)
			{
				foreach (var item in ViewBag.POList)
				{
					string st = "";

					st = item.Currency;
					if (st != null)
					{
						st = st.Replace(" ", "");
					}

					if (st == "VND")
					{

						FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result obj = new FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result()
						{
							RECORDNUMBER = item.RECORDNUMBER,
							Title = item.Title,
							ReceiptedDate = item.ReceiptedDate,
							useCount = item.useCount,
							ISBN = item.ISBN,
							InBookNum = item.InBookNum,
							DKCB = item.DKCB,
							ACQUIREDDATE = item.ACQUIREDDATE,
							LocationID = item.LocationID,
							RECOMMENDID = item.RECOMMENDID,
							Year = item.Year,
							Price = item.Price,
							Currency = item.Currency,
							NXB = item.NXB,
							FullPrice = item.FullPrice,
							ItemID = item.ItemID,
							DateLastUsed = item.DateLastUsed
						};
						display1.Add(obj);
						
						
					}
					if (st == "YEN")
					{
						FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result obj = new FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result()
						{
							RECORDNUMBER = item.RECORDNUMBER,
							Title = item.Title,
							ReceiptedDate = item.ReceiptedDate,
							useCount = item.useCount,
							ISBN = item.ISBN,
							InBookNum = item.InBookNum,
							DKCB = item.DKCB,
							ACQUIREDDATE = item.ACQUIREDDATE,
							LocationID = item.LocationID,
							RECOMMENDID = item.RECOMMENDID,
							Year = item.Year,
							Price = item.Price,
							Currency = item.Currency,
							NXB = item.NXB,
							FullPrice = item.FullPrice,
							ItemID = item.ItemID,
							DateLastUsed = item.DateLastUsed,
						};
						display2.Add(obj);

					}
					if (st == "USD")
					{
						FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result obj = new FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result()
						{
							RECORDNUMBER = item.RECORDNUMBER,
							Title = item.Title,
							ReceiptedDate = item.ReceiptedDate,
							useCount = item.useCount,
							ISBN = item.ISBN,
							InBookNum = item.InBookNum,
							DKCB = item.DKCB,
							ACQUIREDDATE = item.ACQUIREDDATE,
							LocationID = item.LocationID,
							RECOMMENDID = item.RECOMMENDID,
							Year = item.Year,
							Price = item.Price,
							Currency = item.Currency,
							NXB = item.NXB,
							FullPrice = item.FullPrice,
							ItemID = item.ItemID,
							DateLastUsed = item.DateLastUsed,
						};
						display3.Add(obj);
					}
					if (st == "B?NGANH")
					{
						FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result obj = new FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result()
						{
							RECORDNUMBER = item.RECORDNUMBER,
							Title = item.Title,
							ReceiptedDate = item.ReceiptedDate,
							useCount = item.useCount,
							ISBN = item.ISBN,
							InBookNum = item.InBookNum,
							DKCB = item.DKCB,
							ACQUIREDDATE = item.ACQUIREDDATE,
							LocationID = item.LocationID,
							RECOMMENDID = item.RECOMMENDID,
							Year = item.Year,
							Price = item.Price,
							Currency = item.Currency,
							NXB = item.NXB,
							FullPrice = item.FullPrice,
							ItemID = item.ItemID,
							DateLastUsed = item.DateLastUsed
						};
						display4.Add(obj);
					}
					if (st == "CENT")
					{
						FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result obj = new FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result()
						{
							RECORDNUMBER = item.RECORDNUMBER,
							Title = item.Title,
							ReceiptedDate = item.ReceiptedDate,
							useCount = item.useCount,
							ISBN = item.ISBN,
							InBookNum = item.InBookNum,
							DKCB = item.DKCB,
							ACQUIREDDATE = item.ACQUIREDDATE,
							LocationID = item.LocationID,
							RECOMMENDID = item.RECOMMENDID,
							Year = item.Year,
							Price = item.Price,
							Currency = item.Currency,
							NXB = item.NXB,
							FullPrice = item.FullPrice,
							ItemID = item.ItemID,
							DateLastUsed = item.DateLastUsed,
						};
						display5.Add(obj);
					}
					if (st == "EUR")
					{
						FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result obj = new FPT_SP_GET_HOLDING_BY_RECOMMENDID_Newest_Result()
						{
							RECORDNUMBER = item.RECORDNUMBER,
							Title = item.Title,
							ReceiptedDate = item.ReceiptedDate,
							useCount = item.useCount,
							ISBN = item.ISBN,
							InBookNum = item.InBookNum,
							DKCB = item.DKCB,
							ACQUIREDDATE = item.ACQUIREDDATE,
							LocationID = item.LocationID,
							RECOMMENDID = item.RECOMMENDID,
							Year = item.Year,
							Price = item.Price,
							Currency = item.Currency,
							NXB = item.NXB,
							FullPrice = item.FullPrice,
							ItemID = item.ItemID,
							DateLastUsed = item.DateLastUsed,
						};
						display6.Add(obj);
					}
				}
			}
			//check null VND
			if (display1.Count == 0)
			{
				ViewBag.DisVND = null;
			}
			else
			{
				ViewBag.DisVND = display1.ToList();
				

			}
			//check null
			if (display2.Count == 0)
			{
				ViewBag.DisYEN = null;
			}
			else
			{
				ViewBag.DisYEN = display2;
			}
			//check null
			if (display3.Count == 0)
			{
				ViewBag.DisUSD = null;
			}
			else
			{
				ViewBag.DisUSD = display3.ToList();
			}
			//check null
			if (display4.Count == 0)
			{
				ViewBag.DisBAnh = null;
			}
			else
			{
				ViewBag.DisBAnh = display4.ToList();
			}

			//check null
			if (display5.Count == 0)
			{
				ViewBag.DisCENT = null;
			}
			else
			{
				ViewBag.DisCENT = display5.ToList();
			}
			//check null
			if (display6.Count == 0)
			{
				ViewBag.DisEUR = null;
			}
			else
			{
				ViewBag.DisEUR = display6.ToList();
			}


			return View();
		}
		public Temper checkDup(List<Temper> lst, Temper obj)
		{
			Temper temp = null;
			foreach(var item in lst)
			{
				if (item.DKCB==obj.DKCB)
				{
					temp = item;
				}
			}
			return temp;
		}
		[AuthAttribute(ModuleID = 4, RightID = "28")]
		public ActionResult StatisticTop20()
		{
			List<SelectListItem> cat = new List<SelectListItem>
			{
				new SelectListItem { Text = "Hãy chọn tiêu chí", Value = "" }
			};
			foreach (var c in le.CAT_DIC_LIST.ToList())
			{
				cat.Add(new SelectListItem { Text = c.Name.ToString(), Value = c.ID.ToString() });
			}
			ViewData["cat"] = cat;
			return View();
		}

		public PartialViewResult GetTop20Stats(string strCatID)
		{
			int id = 0;
			if (!String.IsNullOrEmpty(strCatID)) id = Int32.Parse(strCatID);
			CAT_DIC_LIST cat = le.CAT_DIC_LIST.Where(a => a.ID == id).First();
			if (cat.ID == 38)
			{
				ViewBag.BAPResult = null;
				ViewBag.DAPResult = null;
			}
			else
			{
				ViewBag.BAPResult = le.FPT_ACQ_STATISTIC_TOP20(1, cat.ID).ToList();
				ViewBag.DAPResult = le.FPT_ACQ_STATISTIC_TOP20(0, cat.ID).ToList();
			}

			ViewBag.Category = cat.Name;
			ViewBag.Total = le.FPT_ACQ_LANGUAGE_STATISTIC(0).First();
			return PartialView("GetTop20Stats");
		}

		//statistic book in
		[AuthAttribute(ModuleID = 4, RightID = "28")]
		public ActionResult StatTaskbar()
		{
			List<SelectListItem> lib = new List<SelectListItem>
			{
				new SelectListItem { Text = "Hãy chọn thư viện", Value = "" }
			};
			foreach (var l in le.SP_HOLDING_LIB_SEL((int)Session["UserID"]).ToList())
			{
				lib.Add(new SelectListItem { Text = l.Code, Value = l.ID.ToString() });
			}
			ViewData["lib"] = lib;
			return View();
		}

		public PartialViewResult GetStatTaskbar(string strLibID, string LocationPrefix, string strLocID, string strFromDate, string strToDate)
		{
			int LibID = 0;
			//int LocID = 0;
			int count = 0;
			if (!String.IsNullOrEmpty(strLibID)) LibID = Convert.ToInt32(strLibID);
			//if (!String.IsNullOrEmpty(strLocID)) LocID = Convert.ToInt32(strLocID);
			List<FPT_SP_GET_ITEM_Result> listItem = new List<FPT_SP_GET_ITEM_Result>();
			List<FPT_SP_GET_ITEM_Result> listIte = new List<FPT_SP_GET_ITEM_Result>();

			listIte = ab.FPT_SP_GET_ITEM_LIST(strFromDate, strToDate, strLocID, LocationPrefix, LibID).ToList();

			foreach (var item in listIte)
			{
				count++;
				int countCopy = 0;
				int borrowNum = 0;
				int remainingNum = 0;
				string tGia = "";
				string noiXB = "";
				string nhaXB = "";
				string namXB = "";
				int nam = 0;
				string StrTitle = "";
				string Strdigit = "";
				//lay thong tin item
				foreach (var items in ab.FPT_SP_GET_ITEM_INFOR_LIST(item.ID, strLocID, LocationPrefix, LibID))
				{
					if (items.FieldCode == "100")
					{
						tGia = GetContent(items.Content);
					}
					if (items.FieldCode == "044")
					{
						noiXB = GetContent(items.Content);
						if (noiXB == "cc")
						{
							noiXB = "";
						}
					}
					if (items.FieldCode == "260")
					{
						int vitriC = -1;
						int vitriB = -1;
						vitriC = items.Content.LastIndexOf("$c");
						vitriB = items.Content.IndexOf("$b");
						if (vitriC > -1)
						{
							vitriC = vitriC + 1;
							namXB = items.Content.Substring(vitriC + 1);
							foreach (char value in namXB)
							{
								bool digit = char.IsDigit(value);
								if (digit == true)
								{
									Strdigit = Strdigit + value.ToString();
								}
							}
							namXB = Strdigit;
							if (namXB != "")
							{
								nam = Convert.ToInt32(namXB);
							}

							if (vitriB > -1)
							{
								if (vitriB < vitriC)
								{
									nhaXB = items.Content.Substring(vitriB, vitriC - vitriB - 2);
								}
								else if (vitriB > vitriC)
								{
									nhaXB = items.Content.Substring(vitriB);
								}
								nhaXB = GetContent(nhaXB);
							}
						}
						else
						{
							if (vitriB > -1)
							{
								nhaXB = items.Content.Substring(vitriB + 2);
								nhaXB = GetContent(nhaXB);
							}
							//nhaXB = strtemt;
						}

					}
					StrTitle = item.Content;
					int vitriP = -1;
					vitriP = StrTitle.IndexOf("$p");
					//int vitriN = -1;

					if (vitriP > -1)
					{
						StrTitle = StrTitle.Substring(0, vitriP - 1);
					}
					int vitriTitleC = -1;
					vitriTitleC = StrTitle.IndexOf("$c");
					if (vitriTitleC > -1)
					{
						StrTitle = StrTitle.Substring(0, vitriTitleC - 1);
					}

					StrTitle = GetContent(StrTitle);

					if (items.FieldCode == "luongmuon")
					{
						borrowNum = Convert.ToInt32(items.ItemID);
					}

					if (items.FieldCode == "soluong")
					{
						countCopy = Convert.ToInt32(items.ItemID);
					}

					remainingNum = countCopy - borrowNum;
				}

				//so luong muon

				listItem.Add(
					new FPT_SP_GET_ITEM_Result(
						item.ID, StrTitle, item.Code, tGia, noiXB,
						nhaXB, nam, countCopy, item.DKCB, borrowNum, remainingNum));

			}




			//gop DKCB
			int slDauphay = 0;
			int u = 1;
			int dem = 1;
			int indexGan = 0;
			int demso = 0;
			string ganString = "";
			int slnhap = 0;
			//gộp DKCB
			foreach (var item in listItem)
			{
				string DKCBs = "";
				DKCBs = item.DKCB;
				char key = ',';
				for (int i = 0; i < DKCBs.Length; i++)
				{
					if (DKCBs[i] == key)
					{
						slDauphay++;

					}

				}
				slnhap = item.soluong;
				String[] arrDK = new string[slDauphay + 1];
				String[] arrDKfull = new string[slDauphay + 1];
				string h = item.DKCB;
				String ht = "";
				String strghep = "";
				string lastStr = "";

				if (slnhap > 1)
				{
					int indexDau = DKCBs.IndexOf(',');
					if (indexDau > 0)
					{
						ht = DKCBs.Substring(0, indexDau);
						lastStr = DKCBs.Substring(0, indexDau);
					}
					int bienphu = 0;
					string[] arrDKCBs = new string[slDauphay + 1];
					for (int i = 0; i < slDauphay; i++)
					{
						int checkDau = DKCBs.IndexOf(',');
						if (checkDau > 0)
						{
							string strTempt = DKCBs.Substring(0, checkDau);
							DKCBs = DKCBs.Substring(checkDau + 1);
							strTempt = strTempt.Substring(strTempt.Length - 6, 6);
							arrDKCBs[i] = strTempt;
						}
						bienphu++;

					}
					arrDKCBs[bienphu] = DKCBs.Substring(DKCBs.Length - 6, 6);

					//PHAN CU
					int kp = 0;
					for (int m = 0; m < arrDKCBs.Length; m++)
					{
						int n = m + 1;
						int intM = 0;
						int intN = 0;
						if (n < arrDKCBs.Length)
						{
							string strM = arrDKCBs[m];
							intM = Int32.Parse(strM);
							string strN = arrDKCBs[n];
							intN = Int32.Parse(strN); ;
							kp = intM + 1;
						}

						if (intN == kp)
						{
							if (n < arrDKCBs.Length)
							{
								indexGan = n;
								ganString = arrDKCBs[n];
								ganString = ganString.Substring(4, 2);
								demso++;
							}
							else
							{

							}
						}
						else if (n == arrDKCBs.Length - 1)
						{
							//lastStr = lastStr.Substring(lastStr.Length - 6, 6);
							if (lastStr == ht)
							{
								ganString = arrDKCBs[m];
								ganString = ganString.Substring(4, 2);
								if (indexGan > 0)
								{
									int ck = arrDKCBs.Length;
									if (indexGan == ck)
									{
										strghep = strghep + "-" + ganString;

									}
									else if (indexGan < ck)
									{
										strghep = strghep + "-" + ganString + ",";

									}

								}
								else
								{
									int ck = arrDKCBs.Length;
									if (indexGan == ck)
									{
										strghep = strghep + "," + ganString;
									}
									else if (indexGan < ck)
									{
										strghep = strghep + ganString + ",";
									}
								}
							}
							else
							{
								ganString = arrDKCBs[m];
								ganString = ganString.Substring(4, 2);
								int sDoi = Int32.Parse(ganString);
								int hieu = 0;
								hieu = sDoi - demso;
								if (hieu < 0)
								{
									hieu = hieu - (2 * hieu);
								}
								if (indexGan > 0)
								{
									int ck = arrDKCBs.Length;
									if (indexGan == ck)
									{

										strghep = strghep + hieu + "-" + ganString;
									}
									else if (indexGan < ck)
									{
										strghep = strghep + hieu + "-" + ganString + ",";
									}

								}
								else
								{
									int ck = arrDKCBs.Length;
									if (indexGan == ck)
									{
										strghep = strghep + "," + ganString;
									}
									else if (indexGan < ck)
									{
										strghep = strghep + ganString + ",";
									}
								}
							}
							ht = ganString;
							indexGan = 0;
							demso = 0;
						}
						else
						{
							if (lastStr == ht)
							{
								ganString = arrDKCBs[m];
								ganString = ganString.Substring(4, 2);
								if (indexGan > 0)
								{
									int ck = arrDKCBs.Length;
									if (indexGan < ck)
									{
										strghep = strghep + "-" + ganString + ",";

									}

								}
								else
								{
									int ck = arrDKCBs.Length;
									if (indexGan == ck)
									{
										strghep = strghep + ",";
									}
									else if (indexGan < ck)
									{
										strghep = strghep + ",";
									}
								}
							}
							else
							{
								ganString = arrDKCBs[m];
								ganString = ganString.Substring(4, 2);
								int sDoi = Int32.Parse(ganString);
								int hieu = 0;
								hieu = sDoi - demso;
								if (hieu < 0)
								{
									hieu = hieu - (2 * hieu);
								}
								if (indexGan > 0)
								{
									int ck = arrDKCBs.Length;
									if (indexGan == ck)
									{

										strghep = strghep + hieu + "-" + ganString;
									}
									else if (indexGan < ck)
									{
										strghep = strghep + hieu + "-" + ganString + ",";
									}

								}
								else
								{
									int ck = arrDKCBs.Length;
									if (indexGan == ck)
									{
										strghep = strghep + "," + ganString;
									}
									else if (indexGan < ck)
									{
										strghep = strghep + ganString + ",";
									}
								}
							}
							ht = ganString;
							indexGan = 0;
							demso = 0;
						}
						//}
					}
					u = dem;

					//CUOI
					if (strghep.LastIndexOf(',') > 0)
					{
						strghep = strghep.Substring(0, strghep.LastIndexOf(','));
					}

					item.DKCB = lastStr + strghep;

				}
				else if (slnhap == 1)
				{
					int hjk = 0;
					hjk = DKCBs.IndexOf(',');
					if (hjk == -1)
					{
						item.DKCB = DKCBs;
					}
					else
					{
						item.DKCB = DKCBs.Substring(0, hjk);
					}
					u++;

				}

				slDauphay = 0;
			}

			if (listItem.Count > 0)
			{
				ViewBag.Result = listItem;
			}
			else
			{
				ViewBag.Result = null;
			}

			return PartialView("GetStatTaskbar");
		}


		////kiem ke
		//public ActionResult InventoryReport()
		//{
		//	List<SelectListItem> lib = new List<SelectListItem>();
		//	lib.Add(new SelectListItem { Text = "Hãy chọn thư viện", Value = "" });
		//	foreach (var l in le.SP_HOLDING_LIB_SEL((int)Session["UserID"]).ToList())
		//	{
		//		lib.Add(new SelectListItem { Text = l.Code, Value = l.ID.ToString() });
		//	}
		//	ViewData["lib"] = lib;
		//	return View();
		//}

		//public PartialViewResult GetInventoryReport(string strLibID, string strDKCBID)
		//{
		//	strDKCBID = strDKCBID.Trim();
		//	string[] myList = strDKCBID.Split('\n');
		//	int countCN = myList.Length;
		//	int libid = 0;
		//	if (strLibID != "")
		//	{
		//		libid = Convert.ToInt32(strLibID);
		//	}
		//	int cirCount = 0;
		//	int totalInLoc = 0, totalReLoc = 0;
		//	List<FPT_SP_GET_GENERAL_LOC_INFOR_DUCNV_Result> listCountResult = ab.FPT_SP_GET_GENERAL_LOC_INFOR_DUCNV_LIST(libid, 0, null, 1);
		//	foreach (var item in listCountResult)
		//	{
		//		if (item.Type == "CountCir")
		//		{
		//			cirCount = Convert.ToInt32(item.VALUE);
		//		}

		//		if (item.Type == "SUMCOPY")
		//		{
		//			totalInLoc = Convert.ToInt32(item.VALUE);
		//		}
		//	}
		//	totalReLoc = countCN + cirCount;
		//	List<FPT_SP_INVENTORY_Result> listData = le.FPT_SP_INVENTORY(libid).ToList();
		//	// List<FPT_SP_INVENTORY_Result> listLackData = new List<FPT_SP_INVENTORY_Result>();
		//	//List<FPT_SP_INVENTORY_Result> listExcessData = new List<FPT_SP_INVENTORY_Result>();

		//	List<string> listStr = myList.ToList();

		//	if (myList.Length != 0)
		//	{

		//		for (int j = 0; j < listData.Count; j++)
		//		{
		//			for (int i = 0; i < listStr.Count; i++)
		//			{
		//				if (listData[j].CopyNumber == listStr[i])
		//				{
		//					listStr.RemoveAt(i);
		//					listData.RemoveAt(j);
		//				}
		//			}

		//		}


		//	}
		//	if (listData.Count > 0)
		//	{
		//		ViewBag.LackDataResult = listData;
		//	}
		//	else
		//	{
		//		ViewBag.LackDataResult = null;
		//	}

		//	if (listStr.Count > 0)
		//	{
		//		ViewBag.ExcessDataResult = listStr;
		//	}
		//	else
		//	{
		//		ViewBag.ExcessDataResult = null;
		//	}

		//	return PartialView("GetInventoryReport");
		//}

		public ActionResult SpecializedReport()
		{
			List<SelectListItem> lib = new List<SelectListItem>
			{
				new SelectListItem { Text = "Hãy chọn thư viện", Value = "" }
			};
			foreach (var l in le.FPT_SP_CIR_LIB_SEL((int)Session["UserID"]).ToList())
			{
				lib.Add(new SelectListItem { Text = l.Code, Value = l.ID.ToString() });
			}
			ViewData["lib"] = lib;
			return View();
		}

		public PartialViewResult GetSpecializedReport(string strLibID, string strSubjects, string strSpec)
		{
			int LibID = 0;
			if (!String.IsNullOrEmpty(strLibID)) LibID = Int32.Parse(strLibID);
			//;$aPRJ101;$aPRJ201;
			strSubjects = strSubjects.Trim().ToUpper().Replace(" ", ";$a");
			strSubjects = ";$a" + strSubjects + ';';
			int GTTOTAL = 0;
			int TKTOTAL = 0;
			string ItemIDs = "";
			ViewBag.Result = le.FPT_SPECIALIZED_REPORT(LibID, strSubjects, (int)Session["UserID"]).ToList();
			//int count = le.FPT_SPECIALIZED_REPORT(LibID, strSubjects).Select(a => a.ITEMCODE).Distinct().Count();
			foreach (var i in ViewBag.Result)
			{
				ItemIDs += " " + i.ItemID;
				i.SUBJECTCODE = format.OnFormatHoldingTitle(i.SUBJECTCODE);
				string name = i.ITEMNAME;
				i.ITEMNAME = format.getTagOnMarc(name, "a") + " " + format.getTagOnMarc(name, "n");
				//get author $c245
				i.AUTHOR = format.getTagOnMarc(name, "c");
				string isbn = "";
				foreach (var item in le.FPT_JOIN_ISBN(i.ItemID))
				{
					isbn = item.ISBN;
				}
				i.ISBN = isbn;

				string publisher = "";
				foreach (var item in le.FPT_SPECIALIZED_REPORT_GET_PUBLISHER(i.ItemID))
				{
					publisher = item.PUBLISHER;
				}
				//int a = le.FPT_SPECIALIZED_REPORT_COUNT_COPYNUMBER(i.ItemID, LibID, (int)Session["UserID"]);
				i.PUBLISHER = publisher;
				//get publish number
				int itemCheck = (int)i.ItemID;
				List<FPT_SPECIALIZED_REPORT_GET_YEAR_PUBLISHNUM_Result> x = le.FPT_SPECIALIZED_REPORT_GET_YEAR_PUBLISHNUM(itemCheck, 0).ToList();
				if (x.Count > 0)
				{
					string[] arr = x.First().CONTENT.Split('/');
					for (int j = 0; j < arr.Length; j++)
					{
						if (arr[j].Contains("$a"))
						{
							if (arr[j].EndsWith("."))
							{
								arr[j] = arr[j].Substring(0, arr[j].Length - 1);
							}
							i.PUBLISHNUM = arr[j].Replace("$a", "");

						}
					}
				}
				//get year
				List<FPT_SPECIALIZED_REPORT_GET_YEAR_PUBLISHNUM_Result> y = le.FPT_SPECIALIZED_REPORT_GET_YEAR_PUBLISHNUM(itemCheck, 1).ToList();
				if (y.Count > 0)
				{
					string[] arr = y.First().CONTENT.Split('$');
					for (int j = 1; j < arr.Length; j++)
					{
						if (arr[j][0].ToString().Equals("c"))
						{
							i.YEAR = arr[j].Replace("c", "");
						}
					}
				}
				List<FPT_SPECIALIZED_REPORT_GET_GTTK_Result> gtlst = le.FPT_SPECIALIZED_REPORT_GET_GTTK(itemCheck, 0, (int)Session["UserID"], LibID).ToList();
				if (gtlst.Count > 0)
				{
					i.GTNUMBER = gtlst.First().total.ToString();
					GTTOTAL += Int32.Parse(i.GTNUMBER);
				}
				List<FPT_SPECIALIZED_REPORT_GET_GTTK_Result> tklst = le.FPT_SPECIALIZED_REPORT_GET_GTTK(itemCheck, 1, (int)Session["UserID"], LibID).ToList();
				if (tklst.Count > 0)
				{
					i.TKNUMBER = tklst.First().total.ToString();
					TKTOTAL += Int32.Parse(i.TKNUMBER);
				}
			}
			List<FPT_SPECIALIZED_REPORT_Result> lst = ViewBag.Result;
			List<int> deindx = new List<int>();
			List<FPT_SPECIALIZED_REPORT_Result> idx = new List<FPT_SPECIALIZED_REPORT_Result>();
			for (int i = 0; i < lst.Count - 1; i++)
			{
				for (int j = i + 1; j < lst.Count; j++)
				{
					if (lst[i].ITEMCODE.Equals(lst[j].ITEMCODE) && lst[i].ITEMNAME.Equals(lst[j].ITEMNAME)
						&& !lst[i].SUBJECTCODE.Equals(lst[j].SUBJECTCODE))
					{
						lst[i].SUBJECTCODE = lst[i].SUBJECTCODE + " " + lst[j].SUBJECTCODE;
						lst[j].SUBJECTCODE = "";
						if (!lst[j].GTNUMBER.Equals(""))
						{
							GTTOTAL = GTTOTAL - Int32.Parse(lst[j].GTNUMBER);
						}
						if (!lst[j].TKNUMBER.Equals(""))
						{
							TKTOTAL = TKTOTAL - Int32.Parse(lst[j].TKNUMBER);
						}

						deindx.Add(j);

					}
				}
			}

			for (int i = 0; i < deindx.Count; i++)
			{
				lst[deindx[i]] = null;
			}
			for (int i = 0; i < lst.Count; i++)
			{
				if (lst[i] != null)
				{
					idx.Add(lst[i]);
				}
			}
			ViewBag.Result = idx;
			ItemIDs = ItemIDs.Trim().Replace(" ", ";");
			ItemIDs = ";" + ItemIDs + ';';
			ViewBag.GT = le.FPT_SPECIALIZED_REPORT_TOTAL(LibID, ItemIDs, 1, (int)Session["UserID"]).First();
			ViewBag.TK = le.FPT_SPECIALIZED_REPORT_TOTAL(LibID, ItemIDs, 0, (int)Session["UserID"]).First();
			ViewBag.GTItem = le.FPT_SPECIALIZED_REPORT_TOTAL_ITEM(LibID, ItemIDs, 1, (int)Session["UserID"]).First();
			ViewBag.TKItem = le.FPT_SPECIALIZED_REPORT_TOTAL_ITEM(LibID, ItemIDs, 0, (int)Session["UserID"]).First();
			ViewBag.TT = ViewBag.GT + ViewBag.TK;
			ViewBag.TTItem = ViewBag.GTItem;//+ ViewBag.TKItem;
			ViewBag.GTTOTAL = GTTOTAL;
			ViewBag.TKTOTAL = TKTOTAL;
			ViewBag.Spec = strSpec;
			return PartialView("GetSpecializedReport");
		}

		public ActionResult CreateNewSpecialized()
		{
			List<SelectListItem> lib = new List<SelectListItem>
			{
				new SelectListItem { Text = "Hãy chọn thư viện", Value = "" }
			};
			foreach (var l in le.FPT_SP_CIR_LIB_SEL((int)Session["UserID"]).ToList())
			{
				lib.Add(new SelectListItem { Text = l.Code, Value = l.ID.ToString() });
			}
			ViewData["lib"] = lib;
			return View();
		}
	
		[HttpPost]
		public JsonResult GetCreateNewSpecialized(string strLibID, string strSubjects, string strSpec)
		{
			int LibID = 0;
			if (!String.IsNullOrEmpty(strLibID)) LibID = Int32.Parse(strLibID);
			//;$aPRJ101;$aPRJ201;
			strSubjects = strSubjects.Trim().ToUpper();
			List<string> lstName = le.FPT_GET_SPELIALIZED_NAME(LibID).ToList();
			List<String> listtemp = new List<String>();
			foreach (string spc in lstName)
			{
				if (spc.Equals(strSpec.Trim()))
				{
					listtemp.Add(spc);
				}
			}
			if (listtemp.Count == 0)
			{
				le.ExcuteSQL("INSERT INTO FPT_SP_SPECIALIZED_STORE VALUES (N'" + strSpec + "','" + strSubjects + "','" + strLibID + "')");
				ViewBag.message = "Thêm chuyên ngành thành công!!";
			}
			else
			{
				ViewBag.message = "Chuyên ngành đã tồn tại!!";
			}
			return Json(ViewBag.message, JsonRequestBehavior.AllowGet);
		}

		public JsonResult GetSpecializedStored(string id)
		{
			List<SelectListItem> sprecial = new List<SelectListItem>();
			sprecial.Add(new SelectListItem { Text = "Chuyên ngành", Value = "0" });
			if (!string.IsNullOrEmpty(id))
			{
				foreach (var lp in le.FPT_GET_SPECIALIZED_STORE(Int32.Parse(id)))
				{
					sprecial.Add(new SelectListItem { Text = lp.Name, Value = lp.ID.ToString() });
				}
			}
			return Json(new SelectList(sprecial, "Value", "Text"));
		}
		[HttpPost]
		public JsonResult GetSpecializedSubject(string id)
		{
			string subjects = "";

			int ID = Int32.Parse(id);
			if (ID != 0)
			{
				FPT_SP_SPECIALIZED_STORE ab = le.FPT_SP_SPECIALIZED_STORE.Where(a => a.ID == ID).First();
				subjects = ab.Name + "/" + ab.Subjects;
			}

			return Json(subjects, JsonRequestBehavior.AllowGet);
		}
		

		public ActionResult UpdateSpecialized()
		{
			List<SelectListItem> lib = new List<SelectListItem>
			{
				new SelectListItem { Text = "Hãy chọn thư viện", Value = "" }
			};
			foreach (var l in le.FPT_SP_CIR_LIB_SEL((int)Session["UserID"]).ToList())
			{
				lib.Add(new SelectListItem { Text = l.Code, Value = l.ID.ToString() });
			}
			ViewData["lib"] = lib;
			return View();
		}
		[HttpPost]
		public JsonResult GetUpdateSpecialized(string strLibID, string strSpec,string strSpecNew)
		{
			int LibID = 0;
			if (!String.IsNullOrEmpty(strLibID)) LibID = Int32.Parse(strLibID);
			//;$aPRJ101;$aPRJ201;
			
			List<string> lstName = le.FPT_GET_SPELIALIZED_NAME(LibID).ToList();
			List<String> listtemp = new List<String>();
			foreach (string spc in lstName)
			{
				if (spc.Equals(strSpecNew.Trim()))
				{
					listtemp.Add(spc);
				}
			}
			if (listtemp.Count == 0)
			{
				le.ExcuteSQL("UPDATE FPT_SP_SPECIALIZED_STORE SET Name=N'" + strSpecNew + "'" + " WHERE ID='" + strSpec + "'");
				ViewBag.message = "Cập nhật chuyên ngành thành công!!";
			}
			else
			{
				ViewBag.message = "Chuyên ngành đã tồn tại!!";
			}



			return Json(ViewBag.message, JsonRequestBehavior.AllowGet);
		}
		[HttpPost]
		public JsonResult GetDeleteSpecialized( string strSpec)
		{
			le.ExcuteSQL("DELETE FROM FPT_SP_SPECIALIZED_STORE WHERE ID='" + strSpec + "'");
			ViewBag.message = "Xóa chuyên ngành thành công!!";

			return Json(ViewBag.message, JsonRequestBehavior.AllowGet);
		}
		public ActionResult AddSubjectToSpecialized()
		{
			List<SelectListItem> lib = new List<SelectListItem>
			{
				new SelectListItem { Text = "Hãy chọn thư viện", Value = "" }
			};
			foreach (var l in le.FPT_SP_CIR_LIB_SEL((int)Session["UserID"]).ToList())
			{
				lib.Add(new SelectListItem { Text = l.Code, Value = l.ID.ToString() });
			}
			ViewData["lib"] = lib;
			return View();
		}
		[HttpPost]
		public JsonResult GetAddSubjectToSpecialized(string strSpec, string strNewSbj)
		{
			strNewSbj = strNewSbj.Trim();
				le.ExcuteSQL("UPDATE FPT_SP_SPECIALIZED_STORE SET Subjects=N'" + strNewSbj + "'" + " WHERE ID='" + strSpec + "'");
				ViewBag.message = "Cập nhật chuyên ngành thành công!!";

			return Json(ViewBag.message, JsonRequestBehavior.AllowGet);
		}
		public ActionResult DeleteSubjectFromSpecialzed()
		{
			List<SelectListItem> lib = new List<SelectListItem>
			{
				new SelectListItem { Text = "Hãy chọn thư viện", Value = "" }
			};
			foreach (var l in le.FPT_SP_CIR_LIB_SEL((int)Session["UserID"]).ToList())
			{
				lib.Add(new SelectListItem { Text = l.Code, Value = l.ID.ToString() });
			}
			ViewData["lib"] = lib;
			return View();
		}
		[HttpPost]
		public JsonResult GetDeleteSubjectFromSpecialzed(string strSpec, string strNewSbj)
		{

			le.ExcuteSQL("UPDATE FPT_SP_SPECIALIZED_STORE SET Subjects=N'" + strNewSbj + "'" + " WHERE ID='" + strSpec + "'");
			ViewBag.message = "Cập nhật chuyên ngành thành công!!";

			return Json(ViewBag.message, JsonRequestBehavior.AllowGet);
		}
		public JsonResult GetLocationsPrefix(string id)
		{
			List<SelectListItem> LocPrefix = new List<SelectListItem>();
			LocPrefix.Add(new SelectListItem { Text = "Tất cả", Value = "0" });
			if (!string.IsNullOrEmpty(id))
			{
				foreach (var lp in le.FPT_CIR_GET_LOCLIBUSER_PREFIX_SEL((int)Session["UserID"], Int32.Parse(id)))
				{
					LocPrefix.Add(new SelectListItem { Text = Regex.Replace(lp.ToString(), @"[^0-9a-zA-Z]+", ""), Value = lp.ToString() });
				}
			}
			return Json(new SelectList(LocPrefix, "Value", "Text"));
		}

		//GET LOCATIONS BY LOCATION PREFIX, LIBRARY, USERID
		public JsonResult GetLocationsByPrefix(int id, string prefix)
		{
			List<SelectListItem> LocByPrefix = new List<SelectListItem>();
			LocByPrefix.Add(new SelectListItem { Text = "Tất cả", Value = "" });

			foreach (var lbp in le.FPT_CIR_GET_LOCFULLNAME_LIBUSER_SEL((int)Session["UserID"], id, prefix))
			{
				LocByPrefix.Add(new SelectListItem { Text = lbp.Symbol, Value = lbp.ID.ToString() });
			}
			return Json(new SelectList(LocByPrefix, "Value", "Text"));
		}
	}


	public class FPT_GET_LIQUIDBOOKS_Result_2
	{
		public string Reason { get; set; }
		public string Content { get; set; }
		public Nullable<System.Int32> AcquiredSourceID { get; set; }
		public string CallNumber { get; set; }
		public string CopyNumber { get; set; }
		public int ID { get; set; }
		public int ItemID { get; set; }
		public int LibID { get; set; }
		public string LiquidCode { get; set; }
		public int LoanType { get; set; }
		public int LocID { get; set; }
		public Nullable<System.Int32> POID { get; set; }
		public string Price { get; set; }
		public string Shelf { get; set; }
		public Nullable<System.Int32> UseCount { get; set; }
		public string Volumn { get; set; }
		public string AcquiredDate { get; set; }
		public string RemovedDate { get; set; }
		public string DateLastUsed { get; set; }
		public string LibName { get; set; }
		public string LocName { get; set; }
	}
}