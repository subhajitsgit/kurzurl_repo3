using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace KurzUrl.Repository.Models;

[Table("tbl_UrlDetail", Schema = "dbo")]
public partial class TblUrlDetail
{
    public int Id { get; set; }
    public string? Title { get; set; }

    public string? MainUrl { get; set; }

    public string? ShortUrl { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public string? Test { get; set; }
}
