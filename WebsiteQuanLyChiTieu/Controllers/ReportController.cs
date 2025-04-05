using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebsiteQuanLyChiTieu.Models;
using WebsiteQuanLyChiTieu.Models.ViewModels;
using WebsiteQuanLyChiTieu.Repositories;

[Authorize]
public class ReportController : Controller
{
    private readonly IRepository<Transaction> _transactionRepository;

    public ReportController(IRepository<Transaction> transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<IActionResult> Index()
    {
        var transactions = await _transactionRepository.GetAllAsync();

        var reportViewModel = new ReportViewModel
        {
            Transactions = transactions.ToList(),
        };

        return View(reportViewModel);
    }
}
