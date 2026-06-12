using Microsoft.Extensions.Logging;
using OrderHub.Domain;
using OrderHub.Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderHub.Application.Services
{
    public class ProductApplicationService
    {
        readonly IUnitOfWork _unitOfWork;
        public ProductApplicationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Product> GetProductById(int id)
        {
            return await _unitOfWork.Products.GetByIdAsync(id);
        }
    }
}
