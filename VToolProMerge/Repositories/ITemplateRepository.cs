using System.Collections.Generic;
using VToolProMerge.Models;

namespace VToolProMerge.Repositories;

public interface ITemplateRepository
{
    IReadOnlyList<TemplateItem> GetAll();
    TemplateItem? GetById(string id);
    void Add(TemplateItem item);
    void Update(TemplateItem item);
    void Delete(string id);
}
