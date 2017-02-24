﻿using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Common;
using DoddleReport;
using DoddleReport.Web;
using IServices.Infrastructure;
using IServices.ISysServices;
using Models.SysModels;
using Web.Helpers;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using IServices.ITaskServices;
using Models.TaskModels;
using Web.Areas.Platform.Helpers;

namespace Web.Areas.Platform.Controllers
{
    /// <summary>
    /// 任务中心
    /// </summary>
    public class TaskCenterController : Controller
    {
        private readonly ITaskCenterService _TaskCenterService;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TaskCenterService"></param>
        /// <param name="unitOfWork"></param>
        public TaskCenterController(ITaskCenterService TaskCenterService, IUnitOfWork unitOfWork)
        {
            _TaskCenterService = TaskCenterService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="ordering"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public ActionResult Index(string keyword, string ordering, int pageIndex = 1)
        {
            var model =
                _TaskCenterService.GetAll()
                                 .Select(
                                     a =>
                                     new { TaskType = a.TaskType.ToString(), a.Title, a.Content,a.Files, TaskExecutor= a.TaskExecutor.UserName, a.UserCreatedBy.UserName, a.ScheduleEndTime, a.Id }).Search(keyword);

            if (!string.IsNullOrEmpty(ordering))
            {
                model = model.OrderBy(ordering, null);
            }

            return View(model.ToPagedList(pageIndex));
        }

        /// <summary>
        /// 导出全部数据
        /// </summary>
        /// <returns></returns>
        public ReportResult Report(string keyword)
        {
            var model =
             _TaskCenterService.GetAll()
                              .Select(
                                  a =>
                                  new { TaskType = a.TaskType.ToString(), a.Title, a.Content, a.Files, TaskExecutor = a.TaskExecutor.UserName, a.UserCreatedBy.UserName, a.ScheduleEndTime, a.Id }).Search(keyword);

            var report = new Report(model.ToReportSource());

            report.DataFields["Id"].Hidden = true;
            report.TextFields.Footer = ConfigurationManager.AppSettings["Copyright"];

            return new ReportResult(report);
        }

        /// <summary>
        /// 查看
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Details(string id)
        {
            var item = _TaskCenterService.GetById(id);
            return View(item);
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return RedirectToAction("Edit");
        }

        //
        // GET: /Platform/TaskCenter/Edit/5

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Edit(string id)
        {
            var item = new TaskCenter();
            if (!string.IsNullOrEmpty(id))
            {
                item = _TaskCenterService.GetById(id);
            }

            // 根据类型选择不同的编辑视图
            // 1、任务发布人


            // 2、任务执行人

            return View(item);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="id"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Edit(string id, TaskCenter collection)
        {
            if (!ModelState.IsValid)
            {
                Edit(id);
                return View(collection);
            }

            _TaskCenterService.Save(id, collection);

            await _unitOfWork.CommitAsync();

            return new EditSuccessResult(id);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<ActionResult> Delete(string id)
        {
            _TaskCenterService.Delete(id);

            await _unitOfWork.CommitAsync();

            return new DeleteSuccessResult();
        }
    }
}