package com.yourapp.recipes.di

import android.content.Context
import androidx.room.Room
import com.yourapp.recipes.data.local.database.RecipeDatabase
import com.yourapp.recipes.data.local.database.Migrations
import com.yourapp.recipes.data.local.dao.*
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.android.qualifiers.ApplicationContext
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
object DatabaseModule {
    
    @Provides
    @Singleton
    fun provideRecipeDatabase(
        @ApplicationContext context: Context,
        callback: RecipeDatabase.Callback
    ): RecipeDatabase {
        return Room.databaseBuilder(
            context,
            RecipeDatabase::class.java,
            RecipeDatabase.DATABASE_NAME
        )
            .addCallback(callback)
            .addMigrations(*Migrations.ALL_MIGRATIONS)
            .fallbackToDestructiveMigration()
            .build()
    }
    
    @Provides
    fun provideRecipeDao(database: RecipeDatabase): RecipeDao {
        return database.recipeDao()
    }
    
    @Provides
    fun provideIngredientDao(database: RecipeDatabase): IngredientDao {
        return database.ingredientDao()
    }
    
    @Provides
    fun provideMealPlanDao(database: RecipeDatabase): MealPlanDao {
        return database.mealPlanDao()
    }
    
    @Provides
    fun provideShoppingListDao(database: RecipeDatabase): ShoppingListDao {
        return database.shoppingListDao()
    }
    
    @Provides
    fun provideRecipeCollectionDao(database: RecipeDatabase): RecipeCollectionDao {
        return database.recipeCollectionDao()
    }
    
    @Provides
    fun provideRecipeStepDao(database: RecipeDatabase): RecipeStepDao {
        return database.recipeStepDao()
    }
    
    @Provides
    fun provideUserPreferencesDao(database: RecipeDatabase): UserPreferencesDao {
        return database.userPreferencesDao()
    }
}
