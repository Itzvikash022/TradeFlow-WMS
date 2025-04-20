using System;
using System.Collections.Generic;

namespace WMS_Application.Models;

public partial class TblStock
{
    public int StockId { get; set; }

    public int? ProductId { get; set; }

    public int? ShopId { get; set; }

    public int Quantity { get; set; }

    public decimal ShopPrice { get; set; }
    public int BoughtPrice { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual TblProduct? Product { get; set; }

    public virtual TblShop? Shop { get; set; }
}
