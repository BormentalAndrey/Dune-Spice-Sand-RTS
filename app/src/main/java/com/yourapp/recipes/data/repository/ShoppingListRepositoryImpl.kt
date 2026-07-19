package com.yourapp.recipes.data.repository

import com.yourapp.recipes.data.local.dao.CategorySpending
import com.yourapp.recipes.data.local.dao.ShoppingListDao
import com.yourapp.recipes.data.local.database.entity.ShoppingItemEntity
import com.yourapp.recipes.domain.model.Ingredient
import com.yourapp.recipes.domain.model.ShoppingItem
import com.yourapp.recipes.domain.repository.ShoppingListRepository
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map
import java.util.*
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class ShoppingListRepositoryImpl @Inject constructor(
    private val shoppingListDao: ShoppingListDao
) : ShoppingListRepository {
    
    override fun getAllItems(): Flow<List<ShoppingItem>> {
        return shoppingListDao.getAllItems().map { entities ->
            entities.map { it.toDomain() }
        }
    }
    
    override fun getItemsGroupedByCategory(): Flow<Map<String, List<ShoppingItem>>> {
        return shoppingListDao.getAllItems().map { entities ->
            entities.map { it.toDomain() }.groupBy { it.category }
        }
    }
    
    override fun getTotalRemainingPrice(): Flow<Float> {
        return shoppingListDao.getTotalRemainingPrice()
    }
    
    override fun getTotalSpentSince(startDate: Long): Flow<Float> {
        return shoppingListDao.getTotalSpentSince(startDate)
    }
    
    override fun getSpendingByCategory(startDate: Long): Flow<List<CategorySpending>> {
        return shoppingListDao.getSpendingByCategory(startDate)
    }
    
    override suspend fun addItem(item: ShoppingItem) {
        shoppingListDao.insertItem(item.toEntity())
    }
    
    override suspend fun addIngredientsToShoppingList(
        ingredients: List<Ingredient>,
        servings: Int,
        recipeId: Long?
    ) {
        val items = ingredients.map { ingredient ->
            ShoppingItemEntity(
                name = ingredient.name,
                quantity = ingredient.quantity * servings,
                unit = ingredient.unit,
                category = ingredient.category,
                recipeId = recipeId
            )
        }
        shoppingListDao.insertItems(items)
    }
    
    override suspend fun updatePurchasedStatus(itemId: Long, isPurchased: Boolean) {
        val purchaseDate = if (isPurchased) System.currentTimeMillis() else null
        shoppingListDao.updatePurchasedStatus(itemId, isPurchased, purchaseDate ?: 0L)
    }
    
    override suspend fun updateItemPrice(itemId: Long, price: Float) {
        shoppingListDao.updateItemPrice(itemId, price)
    }
    
    override suspend fun deleteItem(itemId: Long) {
        shoppingListDao.deleteItem(
            ShoppingItemEntity(id = itemId, name = "", quantity = 0f, unit = "")
        )
    }
    
    override suspend fun clearPurchasedItems() {
        shoppingListDao.deletePurchasedItems()
    }
    
    override suspend fun resetAllItems() {
        shoppingListDao.resetAllItems()
    }
    
    override suspend fun generateFromMealPlan(recipeIds: List<Long>, servings: Map<Long, Int>) {
        // Логика генерации списка покупок из плана питания
    }
    
    private fun ShoppingItemEntity.toDomain(): ShoppingItem {
        return ShoppingItem(
            id = id,
            name = name,
            quantity = quantity,
            unit = unit,
            category = category,
            isPurchased = isPurchased,
            recipeId = recipeId,
            price = price,
            purchaseDate = purchaseDate
        )
    }
    
    private fun ShoppingItem.toEntity(): ShoppingItemEntity {
        return ShoppingItemEntity(
            id = id,
            name = name,
            quantity = quantity,
            unit = unit,
            category = category,
            isPurchased = isPurchased,
            recipeId = recipeId,
            price = price,
            purchaseDate = purchaseDate
        )
    }
}
