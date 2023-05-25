﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using UpdateChecker.Interfaces;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.TaskBar;
using XHS.Common.Events;
using XHS.Common.Events.Model;
using XHS.Models.Events;
using XHS.Service.Log;
using XHS.Spider.Helpers;
using XHS.Spider.ViewModels;

namespace XHS.Spider.Services
{
    public class SearchService
    {
        private static readonly Service.Log.ILogger Logger = LoggerService.Get(typeof(SearchService));

        public static SearchService searchService = null;
        private static IEventAggregator _aggregator { get; set; }
        private static INavigation _navigation;
        private static IServiceProvider _serviceProvider;
        private static IPageServiceNew _pageServiceNew;
        private static WebView2 _webView;
        private SearchService(WebView2 webView, IEventAggregator aggregator, INavigation navigation, IServiceProvider serviceProvider, IPageServiceNew pageServiceNew) {
            _webView = webView; 
            _aggregator = aggregator;
            _navigation = navigation;
            _serviceProvider = serviceProvider;
            _pageServiceNew = pageServiceNew;

        }
        public static SearchService GetSearchService(WebView2 webView, IEventAggregator aggregator, INavigation navigation, IServiceProvider serviceProvider, IPageServiceNew pageServiceNew)
        {
            if (searchService == null) searchService = new SearchService(webView,aggregator,navigation,serviceProvider,pageServiceNew);
            return searchService;
        }
        /// <summary>
        /// 解析支持的输入
        /// </summary>
        /// <param name="input"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void SearchInput(string input)
        {
           
            if (input.Contains("user/profile"))
            {
                try
                {
                    _webView.CoreWebView2.Navigate(input);
                    //订阅事件
                    _aggregator.GetEvent<NavigationCompletedEvent>().Subscribe(Navigation);

                }
                catch (Exception ex)
                {
                    Logger.Error("webView跳转失败：", ex);
                }
            }
            else if (true)
            {

            }
        }

        private static void Navigation(RedirectInfo redirectInfo)
        {
            if (redirectInfo.Url.Contains("user/profile"))
            {
                SetJumpParam(redirectInfo.Url, _serviceProvider, _pageServiceNew, _webView);
                //消事件注册
                _aggregator.GetEvent<NavigationCompletedEvent>().Unsubscribe(Navigation);
                _navigation.Navigate(typeof(Views.Pages.UserProfilePage));
            }
        }
        private static void SetJumpParam(string input, IServiceProvider serviceProvider, IPageServiceNew pageServiceNew, WebView2 webView)
        {
            pageServiceNew.Scope = serviceProvider.CreateScope();
            var dc = pageServiceNew.Scope.ServiceProvider.GetRequiredService<UserProfileViewModel>();
            dc.InputText = input;
            dc.webView = webView;
            //TODO:webView加载完成后再调用初始化数据
            dc.ExecuteInitData();
        }
        /// <summary>
        /// 从url中获取id
        /// </summary>
        /// <param name="input"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public static string GetId(string input, string baseUrl)
        {
            if (!IsUrl(input)) { return ""; }

            string url = EnableHttps(input);
            url = DeleteUrlParam(url);
            return url.Replace(baseUrl, "");
        }

        /// <summary>
        /// 是否为网址
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static bool IsUrl(string input)
        {
            return input.StartsWith("http://") || input.StartsWith("https://");
        }

        /// <summary>
        /// 将http转为https
        /// </summary>
        /// <returns></returns>
        private static string EnableHttps(string url)
        {
            if (!IsUrl(url)) { return null; }

            return url.Replace("http://", "https://");
        }

        /// <summary>
        /// 去除url中的参数
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static string DeleteUrlParam(string url)
        {
            string[] strList = url.Split('?');

            return strList[0].EndsWith("/") ? strList[0].TrimEnd('/') : strList[0];
        }
    }
}
