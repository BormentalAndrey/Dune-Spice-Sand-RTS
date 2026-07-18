package com.yourapp.recipes.presentation.ui.navigation

import androidx.compose.runtime.Composable
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import androidx.navigation.navArgument
import com.yourapp.recipes.presentation.ui.home.HomeScreen
import com.yourapp.recipes.presentation.ui.detail.RecipeDetailScreen
import com.yourapp.recipes.presentation.ui.favorites.FavoritesScreen
import com.yourapp.recipes.presentation.ui.shopping.ShoppingListScreen
import com.yourapp.recipes.presentation.ui.planner.MealPlannerScreen
import com.yourapp.recipes.presentation.ui.settings.SettingsScreen
import com.yourapp.recipes.presentation.ui.edit.RecipeEditScreen

/**
 * Граф навигации приложения.
 * Определяет все маршруты и связи между экранами.
 */
sealed class Screen(val route: String) {
    object Home : Screen("home")
    object RecipeDetail : Screen("recipe_detail/{recipeId}") {
        fun createRoute(recipeId: Long) = "recipe_detail/$recipeId"
    }
    object RecipeEdit : Screen("recipe_edit/{recipeId}") {
        fun createRoute(recipeId: Long = 0L) = "recipe_edit/$recipeId"
    }
    object Favorites : Screen("favorites")
    object ShoppingList : Screen("shopping_list")
    object MealPlanner : Screen("meal_planner")
    object Settings : Screen("settings")
    object About : Screen("about")
}

@Composable
fun RecipesNavGraph() {
    val navController = rememberNavController()
    
    NavHost(
        navController = navController,
        startDestination = Screen.Home.route
    ) {
        composable(Screen.Home.route) {
            HomeScreen(
                onRecipeClick = { recipeId ->
                    navController.navigate(Screen.RecipeDetail.createRoute(recipeId))
                },
                onAddRecipeClick = {
                    navController.navigate(Screen.RecipeEdit.createRoute())
                },
                onFavoritesClick = {
                    navController.navigate(Screen.Favorites.route)
                },
                onShoppingListClick = {
                    navController.navigate(Screen.ShoppingList.route)
                },
                onMealPlannerClick = {
                    navController.navigate(Screen.MealPlanner.route)
                },
                onSettingsClick = {
                    navController.navigate(Screen.Settings.route)
                }
            )
        }
        
        composable(
            route = Screen.RecipeDetail.route,
            arguments = listOf(
                navArgument("recipeId") { type = NavType.LongType }
            )
        ) { backStackEntry ->
            val recipeId = backStackEntry.arguments?.getLong("recipeId") ?: 0L
            RecipeDetailScreen(
                recipeId = recipeId,
                onEditClick = {
                    navController.navigate(Screen.RecipeEdit.createRoute(recipeId))
                },
                onBackClick = { navController.popBackStack() }
            )
        }
        
        composable(
            route = Screen.RecipeEdit.route,
            arguments = listOf(
                navArgument("recipeId") { type = NavType.LongType }
            )
        ) { backStackEntry ->
            val recipeId = backStackEntry.arguments?.getLong("recipeId") ?: 0L
            RecipeEditScreen(
                recipeId = recipeId,
                onSaveClick = { navController.popBackStack() },
                onBackClick = { navController.popBackStack() }
            )
        }
        
        composable(Screen.Favorites.route) {
            FavoritesScreen(
                onRecipeClick = { recipeId ->
                    navController.navigate(Screen.RecipeDetail.createRoute(recipeId))
                },
                onBackClick = { navController.popBackStack() }
            )
        }
        
        composable(Screen.ShoppingList.route) {
            ShoppingListScreen(
                onBackClick = { navController.popBackStack() }
            )
        }
        
        composable(Screen.MealPlanner.route) {
            MealPlannerScreen(
                onRecipeClick = { recipeId ->
                    navController.navigate(Screen.RecipeDetail.createRoute(recipeId))
                },
                onBackClick = { navController.popBackStack() }
            )
        }
        
        composable(Screen.Settings.route) {
            SettingsScreen(
                onBackClick = { navController.popBackStack() }
            )
        }
    }
}
