﻿using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System;
using System.Linq;
using Android.Content;
using Android.Runtime;

namespace ControleDeGastos.Android
{
    [Activity(Label = "Controle de Gastos", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private List<Models.Estabelecimento> _estabelecimentos;
        private List<Models.Gasto> _gastos;
        private List<ListViewGroup> _listViewGroups;
        private ListViewAdapter _adapter;

        protected override void OnCreate(Bundle bundle)
        {
            var culture = new System.Globalization.CultureInfo("de-DE");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            base.OnCreate(bundle);

            _gastos = CarregarGastos();

            SetContentView(Resource.Layout.Main);

            var listViewGastos = FindViewById<ExpandableListView>(Resource.Id.listViewGastos);
            _listViewGroups = PrepararListViewGroups(_gastos);
            _adapter = new ListViewAdapter(this, _listViewGroups);
            listViewGastos.SetAdapter(_adapter);

            for (int group = 0; group <= _adapter.GroupCount - 1; group++)
            {
                listViewGastos.ExpandGroup(group);
            }

            listViewGastos.ChildClick += ListViewGastos_ChildClick;
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                var id = data.Extras.GetInt("Id");
                var gasto = _gastos.FirstOrDefault(g => g.Id == id);

                if (gasto != null)
                {
                    gasto.Valor = Convert.ToDecimal(data.Extras.GetDouble("Valor"));
                    var nomeEstabelecimento = data.Extras.GetString("Estabelecimento");
                    var estabelecimento = _estabelecimentos.FirstOrDefault(e => string.Compare(e.Nome, nomeEstabelecimento, StringComparison.InvariantCultureIgnoreCase) == 0);
                    if (estabelecimento != null)
                    {
                        gasto.Estabelecimento = estabelecimento;
                    }
                    _adapter.NotifyDataSetChanged();
                }
            }
        }

        private void ListViewGastos_ChildClick(object sender, ExpandableListView.ChildClickEventArgs e)
        {
            var infoGrupo = _listViewGroups[e.GroupPosition];
            var gasto = infoGrupo.Gastos[e.ChildPosition];
            var intent = new Intent(this, typeof(EditarGastoActivity));
            intent.PutExtra("Id", gasto.Id);
            intent.PutExtra("Data", infoGrupo.Data.Ticks);
            intent.PutExtra("Estabelecimento", gasto.Estabelecimento.Nome);
            intent.PutExtra("Valor", Convert.ToDouble(gasto.Valor));
            StartActivityForResult(intent, 0);
        }

        private List<Models.Gasto> CarregarGastos()
        {
            _estabelecimentos = new List<Models.Estabelecimento>();
            for (int c = 1; c <= 10; c++)
            {
                _estabelecimentos.Add(new Models.Estabelecimento() { Id = c, Nome = string.Format("Estabelecimento {0}", c) });
            }

            var random = new Random();
            var gastos = new List<Models.Gasto>();
            for (int c = 1; c <= 15; c++)
            {
                var data = DateTime.Now.AddDays(random.Next(0, 3));
                var estabelecimento = _estabelecimentos[random.Next(0, _estabelecimentos.Count - 1)];
                var valor = random.Next(1, 50);
                gastos.Add(new Models.Gasto() { Id = c, Data = data, Estabelecimento = estabelecimento, Valor = valor });
            }

            return gastos;
        }

        private List<ListViewGroup> PrepararListViewGroups(List<Models.Gasto> gastos)
        {
            var listViewGroups = new List<ListViewGroup>();

            if (gastos.Any())
            {
                var gastosAgrupados = from gasto in gastos
                                      group gasto by gasto.Data.Date into grupoDeGastos
                                      select new
                                      {
                                          Data = grupoDeGastos.Key,
                                          Gastos = grupoDeGastos
                                      };

                foreach (var gastoAgrupado in gastosAgrupados)
                {
                    var listViewGroup = new ListViewGroup() { Data = gastoAgrupado.Data };
                    listViewGroups.Add(listViewGroup);
                    listViewGroup.Gastos.AddRange(gastoAgrupado.Gastos);
                }
            }

            return listViewGroups;
        }
    }
}

