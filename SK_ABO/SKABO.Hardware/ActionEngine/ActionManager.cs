using SKABO.Hardware.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SKABO.MAI.ErrorSystem;
using SKABO.ResourcesManager;

namespace SKABO.ActionEngine
{
    public class ActionManager
    {
        public bool is_sort = false;
        public static object lockObj = new object();//定义锁
        public static ActionManager actionmanager = null;
        public List<ActionBase> actionlist = new List<ActionBase>();
        public List<ActionBase> actiontemlist = new List<ActionBase>();
        public List<AbstractCanDevice> occupydev_list = new List<AbstractCanDevice>(); 
        public double lasttime = 0;
        public static ActionManager getInstance()
        {
            if (actionmanager == null) actionmanager = new ActionManager();
            return actionmanager;
        }
        public void runLoop(double time)
        {
            lock (lockObj)
            {
                occupydev_list.Clear();
                //运行动作
                double dt = time - lasttime;
                for (int i = 0; i < actionlist.Count; i++)
                {
                    //设备动作冲突解决
                    bool is_occupy = false;
                    bool is_sleep = actionlist[i].getRuningSleepAct().Count != 0;
                    //占用设备不运行
                    System.Diagnostics.Debug.Assert(actionlist[i].node != null);
                    if (findDevInOccList(actionlist[i].node) && is_sleep == false) continue;
                    is_occupy = getAllRuningActionsCount(actionlist[i].node, actionlist[i]) != 0;
                    if (actionlist[i].isstop == false && (is_occupy == false || is_sleep))
                    {
                        if (!actionlist[i].isInit())
                            actionlist[i].init();
                        if (actionlist[i].istimeout == false)
                        {
                            actionlist[i].run(dt);
                            if (is_sleep == false)
                                occupydev_list.Add(actionlist[i].node);
                        }

                    }
                }
                //删除完成的动作
                for (int i = actionlist.Count - 1; i >= 0; i--)
                {
                    bool isdelete = actionlist[i].getIsDelete();
                    if (actionlist[i].isfinish || isdelete)
                    {
                        var actiontem = actionlist[i];
                        if (isdelete)
                        {
                            ExperimentLogic.getInstance().DrodAllPackage(actiontem.exp_pack);
                        }
                        actionlist.Remove(actiontem);
                        actiontem.Destroy();
                    }
                }
                //添加新动作
                actionlist = actionlist.Concat(actiontemlist).ToList<ActionBase>();
                actiontemlist.Clear();
                if (is_sort)
                {
                    actionlist.Sort((a, b) => { return a.sort_index > b.sort_index ? 1 : -1; });
                    is_sort = false;
                }
                lasttime = time;
            }
        }
        public bool findDevInOccList(AbstractCanDevice device)
        {
            foreach(var dev in occupydev_list)
            {
                if (device != null && dev == device)
                    return true;
            }
            return false;
        }
        public void render()
        {

        }
        public void addAction(ActionBase action)
        {
            lock (lockObj)
            {
                actiontemlist.Add(action);
            }
        }
        public void removeAction(ActionBase act)
        {
            act.isstop = true;
            act.isfinish = true;
        }
        public void removeAllActions()
        {
            lock (lockObj)
            {
                for (int i = 0; i < actionlist.Count; i++)
                {
                    removeAction(actionlist[i]);
                }
                for (int i = 0; i < actiontemlist.Count; i++)
                {
                    removeAction(actiontemlist[i]);
                }
            }
        }

        public List<ActionBase> getActions()
        {
            List<ActionBase> actionlisttem = new List<ActionBase>();
            lock (lockObj)
            {
                for (int i = 0; i < actionlist.Count; i++)
                {
                    actionlisttem.Add(actionlist[i]);

                }
                for (int i = 0; i < actiontemlist.Count; i++)
                {
                    actionlisttem.Add(actiontemlist[i]);
                }
            }
            return actionlisttem;
        }

        public List<ActionBase> getActions(int id)
        {
            List<ActionBase> actionlisttem = new List<ActionBase>();
            lock (lockObj)
            {
                for (int i = 0; i < actionlist.Count; i++)
                {
                    if (actionlist[i].getID() == id)
                        actionlisttem.Add(actionlist[i]);

                }
                for (int i = 0; i < actiontemlist.Count; i++)
                {
                    if (actiontemlist[i].getID() == id)
                        actionlisttem.Add(actiontemlist[i]);
                }
            }
            return actionlisttem;
        }

        public List<ActionBase> getActions(AbstractCanDevice node)
        {
            List<ActionBase> actionlisttem = new List<ActionBase>();
            lock (lockObj)
            {
                for (int i = 0; i < actionlist.Count; i++)
                {
                    if (actionlist[i].node == node)
                        actionlisttem.Add(actionlist[i]);
                }
                for (int i = 0; i < actiontemlist.Count; i++)
                {
                    if (actiontemlist[i].node == node)
                        actionlisttem.Add(actiontemlist[i]);
                }
            }
            return actionlisttem;
        }

        public List<ActionBase> getAllActions(AbstractCanDevice node)
        {
            List<ActionBase> action_listret = new List<ActionBase>();
            var action_list = getActions(node);
            lock (lockObj)
            {
                foreach (var act in action_list)
                {
                    if (act.isInit())
                    {
                        if(act.node ==node)
                        {
                            action_listret.Add(act);
                        }
                    }
                }
            }
            return action_listret;
        }

        public List<ActionBase> getAllRuningActions(AbstractCanDevice node)
        {
            List<ActionBase> action_listret = new List<ActionBase>();
            var action_list = getActions(node);
            lock (lockObj)
            {
                foreach (var act in action_list)
                {
                    if (act.getIsStop() == false && act.isInit())
                    {
                        List<ActionBase> action_listtem = new List<ActionBase>();
                        act.getAllRuningAct(node, ref action_listtem);
                        if (action_listtem.Count != 0) action_listret.Add(act);
                    }
                }
            }
            return action_listret;
        }

        public int getAllRuningActionsCount(AbstractCanDevice node, ActionBase act=null)
        {
            List<ActionBase> action_list = getAllRuningActions(node);
            action_list.Remove(act);
            return action_list.Count();
        }

        public int getAllActionsCount(AbstractCanDevice node, ActionBase act = null)
        {
            List<ActionBase> action_list = getActionCount(node);
            action_list.Remove(act);
            return action_list.Count();
        }

        public List<ActionBase> getActionCount(AbstractCanDevice node)
        {
            List<ActionBase> action_listret = new List<ActionBase>();
            var action_list = getActions(node);
            lock (lockObj)
            {
                foreach (var act in action_list)
                {
                    if (act.getIsStop() == false)
                    {
                        List<ActionBase> action_listtem = new List<ActionBase>();
                        act.getAllAct(node, ref action_listtem);
                        if (action_listtem.Count != 0) action_listret.Add(act);
                    }
                }
            }
            return action_listret;
        }
    }
}
