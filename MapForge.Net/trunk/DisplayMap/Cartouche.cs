using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DisplayMap
{
    /// <summary>
    /// Suivez les étapes 1a ou 1b puis 2 pour utiliser ce contrôle personnalisé dans un fichier XAML.
    ///
    /// Étape 1a) Utilisation de ce contrôle personnalisé dans un fichier XAML qui existe dans le projet actif.
    /// Ajoutez cet attribut XmlNamespace à l'élément racine du fichier de balisage où il doit 
    /// être utilisé :
    ///
    ///     xmlns:MyNamespace="clr-namespace:DisplayMap"
    ///
    ///
    /// Étape 1b) Utilisation de ce contrôle personnalisé dans un fichier XAML qui existe dans un autre projet.
    /// Ajoutez cet attribut XmlNamespace à l'élément racine du fichier de balisage où il doit 
    /// être utilisé :
    ///
    ///     xmlns:MyNamespace="clr-namespace:DisplayMap;assembly=DisplayMap"
    ///
    /// Vous devrez également ajouter une référence du projet contenant le fichier XAML
    /// à ce projet et régénérer pour éviter des erreurs de compilation :
    ///
    ///     Cliquez avec le bouton droit sur le projet cible dans l'Explorateur de solutions, puis sur
    ///     "Ajouter une référence"->"Projets"->[Sélectionnez ce projet]
    ///
    ///
    /// Étape 2)
    /// Utilisez à présent votre contrôle dans le fichier XAML.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class Cartouche : Control
    {
        static Cartouche()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Cartouche), new FrameworkPropertyMetadata(typeof(Cartouche)));
        }

		#region Text DP
		/// <summary>
		/// This DP holds the string that gets displayed following the geometry path
		/// </summary>
		public String Text
		{
			get { return (String)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register("Text", typeof(String), typeof(Cartouche),
#if SILVERLIGHT
            new PropertyMetadata(null, new PropertyChangedCallback(OnStringPropertyChanged)));
#else
			new PropertyMetadata(null, new PropertyChangedCallback(OnTextPropertyChanged),
			new CoerceValueCallback(CoerceTextValue)));
#endif

		static void OnTextPropertyChanged ( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			Cartouche label = d as Cartouche;
			if (label == null)
				return;

#if SILVERLIGHT
            // no coerce in silverlight, so this fakes it...
            CoerceTextValue (d, e.NewValue);
#endif

		}

		static object CoerceTextValue ( DependencyObject d, object baseValue )
		{
			if ((String)baseValue == "")
				return null;


			return baseValue;
		}

		#endregion

		#region StrokeThickness DP
		/// <summary>
		/// This DP holds the string that gets displayed following the geometry path
		/// </summary>
		public double StrokeThickness
		{
			get { return (double)GetValue(StrokeThicknessProperty); }
			set { SetValue(StrokeThicknessProperty, value); }
		}

		public static readonly DependencyProperty StrokeThicknessProperty =
			DependencyProperty.Register("StrokeThickness", typeof(double), typeof(Cartouche),
			new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
		#endregion


		#region Radius DP
		/// <summary>
		/// This DP holds the string that gets displayed following the geometry path
		/// </summary>
		public double Radius
		{
			get { return (double)GetValue(RadiusProperty); }
			set { SetValue(RadiusProperty, value); }
		}

		public static readonly DependencyProperty RadiusProperty =
			DependencyProperty.Register("Radius", typeof(double), typeof(Cartouche),
			new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
		#endregion

    }
}
