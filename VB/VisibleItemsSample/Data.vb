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
        Inherits VisibleItemsSample.SalesProductData.SaleItemBase

    End Class

    Public Class BikeReportRangeItem

        Public Property Revenue As Decimal

        Public Property ReportDate As DateTime
    End Class

    Public Class BikeReport

        Public Property BicyclesData As List(Of VisibleItemsSample.BikeReportItem)

        Public Property BicycleRangesData As List(Of VisibleItemsSample.BikeReportRangeItem)
    End Class

    Public Class SalesProductData

        Private Class ProductItemBase

            Public Property Product As String

            Public Property Category As String

            Public Property Price As Decimal
        End Class

        Public Class SaleItemBase

            Public Property Category As String

            Public Property UnitsSold As Integer

            Public Property Revenue As Decimal

            Public Property UnitsSoldTarget As Integer

            <System.ComponentModel.DataAnnotations.DataTypeAttribute(System.ComponentModel.DataAnnotations.DataType.Currency)>
            Public Property RevenueTarget As Decimal

            <System.ComponentModel.DataAnnotations.DisplayFormatAttribute(DataFormatString:="p")>
            Public ReadOnly Property SalesDynamic As Single
                Get
                    Return CSng(((Me.Revenue - Me.RevenueTarget) / Me.Revenue))
                End Get
            End Property

            Public Property ReportDate As DateTime
        End Class

        Public Class SaleItem
            Inherits VisibleItemsSample.SalesProductData.SaleItemBase

            Public Property State As String

            Public Property Product As String

            <System.ComponentModel.DataAnnotations.DataTypeAttribute(System.ComponentModel.DataAnnotations.DataType.Currency)>
            Public Property Price As Decimal
        End Class

        Private Shared rnd As System.Random = New System.Random()

        Private Shared dataField As System.Collections.Generic.List(Of VisibleItemsSample.SalesProductData.SaleItem)

        Private Shared bicyclesReportField As VisibleItemsSample.BikeReport

        Public Shared ReadOnly Property Data As List(Of VisibleItemsSample.SalesProductData.SaleItem)
            Get
                Return If(VisibleItemsSample.SalesProductData.dataField, Function()
                    VisibleItemsSample.SalesProductData.dataField = VisibleItemsSample.SalesProductData.GetSalesData()
                    Return VisibleItemsSample.SalesProductData.dataField
                End Function())
            End Get
        End Property

        Public Shared ReadOnly Property BicyclesReport As BikeReport
            Get
                Return If(VisibleItemsSample.SalesProductData.bicyclesReportField, Function()
                    VisibleItemsSample.SalesProductData.bicyclesReportField = VisibleItemsSample.SalesProductData.GenerateBicyclesReport()
                    Return VisibleItemsSample.SalesProductData.bicyclesReportField
                End Function())
            End Get
        End Property

        Public Shared BikeCategories As System.Collections.Generic.List(Of String) = New System.Collections.Generic.List(Of String)() From {"Mountain", "Hybrid/Cross", "Road", "Comfort", "Youth", "Cruiser", "Electric"}

        Private Shared Function LoadData() As DataSet
            Dim ds As System.Data.DataSet = New System.Data.DataSet()
            Dim uri As System.Uri = New System.Uri(String.Format("/{0};component/Data/Sales.xml", DevExpress.Utils.AssemblyHelper.GetPartialName(GetType(VisibleItemsSample.SalesProductData).Assembly)), System.UriKind.RelativeOrAbsolute)
            ds.ReadXml(System.Windows.Application.GetResourceStream(CType((uri), System.Uri)).Stream)
            Return ds
        End Function

        Private Shared Function CreateProductBase(ByVal dataRow As System.Data.DataRow, ByVal categoryName As String) As Object
            Return New VisibleItemsSample.SalesProductData.ProductItemBase() With {.Price = dataRow.Field(Of Decimal)("ListPrice"), .Product = dataRow.Field(Of String)("Name"), .Category = categoryName}
        End Function

        Private Shared Function GenerateData(ByVal regions As System.Data.DataRowCollection, ByVal products As System.Collections.Generic.IEnumerable(Of VisibleItemsSample.SalesProductData.ProductItemBase)) As List(Of VisibleItemsSample.SalesProductData.SaleItem)
            Dim totalSales As System.Collections.Generic.List(Of VisibleItemsSample.SalesProductData.SaleItem) = New System.Collections.Generic.List(Of VisibleItemsSample.SalesProductData.SaleItem)()
            For Each region As System.Data.DataRow In regions
                Dim state As String = CStr(region("Region"))
                Dim year As Integer = System.DateTime.Today.Year - 1
                For month As Integer = 1 To 12
                    For Each product As VisibleItemsSample.SalesProductData.ProductItemBase In products
                        Dim tsItem As VisibleItemsSample.SalesProductData.SaleItem = New VisibleItemsSample.SalesProductData.SaleItem With {.State = state, .Category = product.Category, .Product = product.Product, .Price = product.Price}
                        Dim dt As System.DateTime = New System.DateTime(year, month, 1)
                        Dim uSold As Integer = VisibleItemsSample.SalesProductData.GetUnitsSold(product.Category)
                        Dim uSoldTarget As Integer = uSold + VisibleItemsSample.SalesProductData.rnd.[Next](-CInt((uSold * 0.2)), CInt((uSold * 0.2)))
                        Dim rev As Decimal = uSold * product.Price
                        Dim revTarget As Decimal = uSoldTarget * product.Price
                        tsItem.Revenue = rev
                        tsItem.RevenueTarget = revTarget
                        tsItem.UnitsSold = uSold
                        tsItem.UnitsSoldTarget = uSoldTarget
                        tsItem.ReportDate = dt
                        totalSales.Add(tsItem)
                    Next
                Next
            Next

            Return totalSales
        End Function

        Private Shared Function GetUnitsSold(ByVal category As String) As Integer
            Dim max As Integer = If(category.Equals("Bikes"), 50, 250)
            Return VisibleItemsSample.SalesProductData.rnd.[Next](1, max)
        End Function

        Private Shared Function GetState(ByVal region As System.Data.DataRow) As String
            Return CStr(region("Region"))
        End Function

        Private Shared Function GetSalesData() As List(Of VisibleItemsSample.SalesProductData.SaleItem)
            Dim dataSet As System.Data.DataSet = VisibleItemsSample.SalesProductData.LoadData()
            Dim products As System.Data.DataTable = dataSet.Tables("Products")
            Dim categories As System.Data.DataTable = dataSet.Tables("Categories")
            Dim regions As System.Data.DataTable = dataSet.Tables("Regions")
            Dim items = From t1 In products.AsEnumerable() Join t2 In categories.AsEnumerable() On t1("CategoryID") Equals t2("CategoryID") Select VisibleItemsSample.SalesProductData.CreateProductBase(t1, CStr(t2("CategoryName")))
            Return VisibleItemsSample.SalesProductData.GenerateData(regions.Rows, items.Cast(Of VisibleItemsSample.SalesProductData.ProductItemBase)())
        End Function

        Private Shared Function GenerateBicyclesReport() As BikeReport
            Dim result As System.Collections.Generic.List(Of VisibleItemsSample.BikeReportItem) = New System.Collections.Generic.List(Of VisibleItemsSample.BikeReportItem)()
            Dim rangeData As System.Collections.Generic.List(Of VisibleItemsSample.BikeReportRangeItem) = New System.Collections.Generic.List(Of VisibleItemsSample.BikeReportRangeItem)()
            Dim year As Integer = System.DateTime.Today.Year - 1
            Dim startDate As System.DateTime = New System.DateTime(year, 1, 1)
            Dim averageMonthSold As Integer = 1700
            Dim averagePrice As Decimal = 900
            Dim [date] As System.DateTime = startDate
            For day As Integer = 1 To 365 Step 7
                Dim revenue As Decimal = 0
                Dim minDay As Integer = VisibleItemsSample.SalesProductData.rnd.[Next](100, 200)
                Dim maxDay As Integer = VisibleItemsSample.SalesProductData.rnd.[Next](250, 300)
                [date] = startDate.AddDays(day)
                For i As Integer = 0 To VisibleItemsSample.SalesProductData.BikeCategories.Count - 1
                    Dim category As String = VisibleItemsSample.SalesProductData.BikeCategories(i)
                    Dim deltaCorrection As Double = 2 * VisibleItemsSample.SalesProductData.rnd.NextDouble() + 0.2
                    Dim tsItem As VisibleItemsSample.BikeReportItem = New VisibleItemsSample.BikeReportItem With {.Category = category}
                    Dim correction As Double = 22 - i * 3 - VisibleItemsSample.SalesProductData.rnd.NextDouble()
                    If day > minDay AndAlso day < maxDay Then correction += deltaCorrection
                    If day > maxDay Then correction -= deltaCorrection
                    Dim uSold As Integer = CInt((averageMonthSold * correction / 100.0))
                    Dim uSoldTarget As Integer = uSold + VisibleItemsSample.SalesProductData.rnd.[Next](-CInt((uSold * 0.2)), CInt((uSold * 0.2)))
                    Dim rev As Decimal = uSold * averagePrice
                    Dim revTarget As Decimal = uSoldTarget * averagePrice
                    tsItem.Revenue = rev
                    tsItem.RevenueTarget = revTarget
                    tsItem.UnitsSold = uSold
                    tsItem.UnitsSoldTarget = uSoldTarget
                    tsItem.ReportDate = [date]
                    revenue += rev
                    result.Add(tsItem)
                Next

                rangeData.Add(New VisibleItemsSample.BikeReportRangeItem() With {.ReportDate = [date], .Revenue = revenue})
            Next

            Return New VisibleItemsSample.BikeReport() With {.BicyclesData = result, .BicycleRangesData = rangeData}
        End Function
    End Class
End Namespace
