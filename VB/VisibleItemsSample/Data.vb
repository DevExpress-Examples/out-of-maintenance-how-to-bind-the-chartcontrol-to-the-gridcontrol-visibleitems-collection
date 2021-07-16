Imports DevExpress.Mvvm.DataAnnotations
Imports DevExpress.Utils
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.Data
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Windows
Imports System.Windows.Media

Namespace VisibleItemsSample
	Public Class BikeReportItem
		Inherits SalesProductData.SaleItemBase

	End Class
	Public Class BikeReportRangeItem
		Public Property Revenue() As Decimal
		Public Property ReportDate() As DateTime
	End Class
	Public Class BikeReport
		Public Property BicyclesData() As List(Of BikeReportItem)
		Public Property BicycleRangesData() As List(Of BikeReportRangeItem)
	End Class
	Public Class SalesProductData
		Private Class ProductItemBase
			Public Property Product() As String
			Public Property Category() As String
			Public Property Price() As Decimal
		End Class
		Public Class SaleItemBase
			Public Property Category() As String
			Public Property UnitsSold() As Integer
			Public Property Revenue() As Decimal
			Public Property UnitsSoldTarget() As Integer
			<DataType(DataType.Currency)>
			Public Property RevenueTarget() As Decimal
			<DisplayFormat(DataFormatString := "p")>
			Public ReadOnly Property SalesDynamic() As Single
				Get
					Return CSng((Revenue - RevenueTarget) / Revenue)
				End Get
			End Property
			Public Property ReportDate() As DateTime
		End Class

		Public Class SaleItem
			Inherits SaleItemBase

			Public Property State() As String
			Public Property Product() As String
			<DataType(DataType.Currency)>
			Public Property Price() As Decimal
		End Class

		Private Shared rnd As New Random()
'INSTANT VB NOTE: The field data was renamed since Visual Basic does not allow fields to have the same name as other class members:
		Private Shared data_Conflict As List(Of SaleItem)
'INSTANT VB NOTE: The field bicyclesReport was renamed since Visual Basic does not allow fields to have the same name as other class members:
		Private Shared bicyclesReport_Conflict As BikeReport

		Public Shared ReadOnly Property Data() As List(Of SaleItem)
			Get
				If data_Conflict IsNot Nothing Then
					Return data_Conflict
				Else
					data_Conflict = GetSalesData()
					Return data_Conflict
				End If
			End Get
		End Property
		Public Shared ReadOnly Property BicyclesReport() As BikeReport
			Get
				If bicyclesReport_Conflict IsNot Nothing Then
					Return bicyclesReport_Conflict
				Else
					bicyclesReport_Conflict = GenerateBicyclesReport()
					Return bicyclesReport_Conflict
				End If
			End Get
		End Property

		Public Shared BikeCategories As New List(Of String)() From {"Mountain", "Hybrid/Cross", "Road", "Comfort", "Youth", "Cruiser", "Electric"}

		Private Shared Function LoadData() As DataSet
			Dim ds As New DataSet()
			Dim uri As New Uri(String.Format("/{0};component/Data/Sales.xml", AssemblyHelper.GetPartialName(GetType(SalesProductData).Assembly)), UriKind.RelativeOrAbsolute)
			ds.ReadXml(Application.GetResourceStream(uri).Stream)
			Return ds
		End Function
		Private Shared Function CreateProductBase(ByVal dataRow As DataRow, ByVal categoryName As String) As Object
			Return New ProductItemBase() With {
				.Price = dataRow.Field(Of Decimal)("ListPrice"),
				.Product = dataRow.Field(Of String)("Name"),
				.Category = categoryName
			}
		End Function
		Private Shared Function GenerateData(ByVal regions As DataRowCollection, ByVal products As IEnumerable(Of ProductItemBase)) As List(Of SaleItem)
			Dim totalSales As New List(Of SaleItem)()
			For Each region As DataRow In regions
				Dim state As String = DirectCast(region("Region"), String)
				Dim year As Integer = DateTime.Today.Year - 1
				For month As Integer = 1 To 12
					For Each product As ProductItemBase In products
						Dim tsItem As New SaleItem With {
							.State = state,
							.Category = product.Category,
							.Product = product.Product,
							.Price = product.Price
						}
						Dim dt As New DateTime(year, month, 1)
						Dim uSold As Integer = GetUnitsSold(product.Category)
						Dim uSoldTarget As Integer = uSold + rnd.Next(-CInt(Math.Truncate(uSold * 0.2)), CInt(Math.Truncate(uSold * 0.2)))
						Dim rev As Decimal = uSold * product.Price
						Dim revTarget As Decimal = uSoldTarget * product.Price

						tsItem.Revenue = rev
						tsItem.RevenueTarget = revTarget
						tsItem.UnitsSold = uSold
						tsItem.UnitsSoldTarget = uSoldTarget
						tsItem.ReportDate = dt

						totalSales.Add(tsItem)
					Next product
				Next month
			Next region
			Return totalSales
		End Function
		Private Shared Function GetUnitsSold(ByVal category As String) As Integer
			Dim max As Integer = If(category.Equals("Bikes"), 50, 250)
			Return rnd.Next(1, max)
		End Function
		Private Shared Function GetState(ByVal region As DataRow) As String
			Return DirectCast(region("Region"), String)
		End Function
		Private Shared Function GetSalesData() As List(Of SaleItem)
			Dim dataSet As DataSet = LoadData()
			Dim products As DataTable = dataSet.Tables("Products")
			Dim categories As DataTable = dataSet.Tables("Categories")
			Dim regions As DataTable = dataSet.Tables("Regions")

			Dim items = From t1 In products.AsEnumerable()
				Join t2 In categories.AsEnumerable() On t1("CategoryID") Equals t2("CategoryID")
				Select CreateProductBase(t1, CStr(t2("CategoryName")))

			Return GenerateData(regions.Rows, items.Cast(Of ProductItemBase)())
		End Function

		Private Shared Function GenerateBicyclesReport() As BikeReport
			Dim result As New List(Of BikeReportItem)()
			Dim rangeData As New List(Of BikeReportRangeItem)()
			Dim year As Integer = DateTime.Today.Year - 1
			Dim startDate As New DateTime(year, 1, 1)
			Dim averageMonthSold As Integer = 1700
			Dim averagePrice As Decimal = 900
			Dim [date] As DateTime = startDate
			For day As Integer = 1 To 365 Step 7
				Dim revenue As Decimal = 0
				Dim minDay As Integer = rnd.Next(100, 200)
				Dim maxDay As Integer = rnd.Next(250, 300)
				[date] = startDate.AddDays(day)
				For i As Integer = 0 To BikeCategories.Count - 1
					Dim category As String = BikeCategories(i)

					Dim deltaCorrection As Double = 2 * rnd.NextDouble() + 0.2

					Dim tsItem As New BikeReportItem With {.Category = category}
					Dim correction As Double = 22 - i * 3 - rnd.NextDouble()
					If day > minDay AndAlso day < maxDay Then
						correction += deltaCorrection
					End If
					If day > maxDay Then
						correction -= deltaCorrection
					End If

					Dim uSold As Integer = CInt(Math.Truncate(averageMonthSold * correction / 100.0))
					Dim uSoldTarget As Integer = uSold + rnd.Next(-CInt(Math.Truncate(uSold * 0.2)), CInt(Math.Truncate(uSold * 0.2)))
					Dim rev As Decimal = uSold * averagePrice
					Dim revTarget As Decimal = uSoldTarget * averagePrice

					tsItem.Revenue = rev
					tsItem.RevenueTarget = revTarget
					tsItem.UnitsSold = uSold
					tsItem.UnitsSoldTarget = uSoldTarget
					tsItem.ReportDate = [date]
					revenue += rev
					result.Add(tsItem)
				Next i

				rangeData.Add(New BikeReportRangeItem() With {
					.ReportDate = [date],
					.Revenue = revenue
				})
			Next day
			Return New BikeReport() With {
				.BicyclesData = result,
				.BicycleRangesData = rangeData
			}
		End Function
	End Class

End Namespace
